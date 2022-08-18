﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NeatNetwork.Libraries;
using NeatNetwork.NetworkFiles;

namespace NeatNetwork.NetworkFiles
{
    internal class LSTMNeuron
    {
        internal double CellState;
        internal double HiddenState;

        internal NeuronConnectionsInfo Weights;
        internal double Bias;

        internal double ForgetWeight;
        internal double StoreSigmoidWeight;
        internal double StoreTanhWeight;
        internal double OutputWeight;

        internal double Execute(List<double[]> previousLayerActivations, out NeuronValues neuronExecutionVals)
        {
            neuronExecutionVals = new NeuronValues(NeuronHolder.NeuronType.LSTM)
            {
                InitialCellState = CellState,
                InitialHiddenState = HiddenState,
            };

            double linearFunction = Bias;
            for (int i = 0; i < Weights.Length; i++)
            {
                Point connectedPos = Weights.ConnectedNeuronsPos[i];
                linearFunction += previousLayerActivations[connectedPos.X][connectedPos.Y] * Weights.Weights[i];
            }
            neuronExecutionVals.LinearFunction = linearFunction;

            double hiddenStateSigmoid = Activation.Sigmoid(HiddenState);

            double forgetGate = hiddenStateSigmoid;
            neuronExecutionVals.AfterForgetGateBeforeForgetWeightMultiplication = forgetGate;

            forgetGate *= ForgetWeight;
            neuronExecutionVals.AfterForgetGateSigmoidAfterForgetWeightMultiplication = forgetGate;

            CellState *= forgetGate;
            neuronExecutionVals.AfterForgetGateMultiplication = CellState;

            double storeGateSigmoidPath = hiddenStateSigmoid;
            neuronExecutionVals.AfterSigmoidStoreGateBeforeStoreWeightMultiplication = storeGateSigmoidPath;
            
            storeGateSigmoidPath *= StoreSigmoidWeight;
            neuronExecutionVals.AfterSigmoidStoreGateAfterStoreWeightMultiplication = storeGateSigmoidPath;

            double storeGateTanhPath = Activation.Tanh(HiddenState);
            neuronExecutionVals.AfterTanhStoreGateBeforeWeightMultiplication = storeGateTanhPath;

            storeGateTanhPath *= StoreTanhWeight;
            neuronExecutionVals.AfterTanhStoreGateAfterWeightMultiplication = storeGateTanhPath;

            double storeGate = storeGateSigmoidPath * storeGateTanhPath;
            neuronExecutionVals.AfterStoreGateMultiplication = storeGate;

            CellState += storeGate;

            double outputGateSigmoidPath = hiddenStateSigmoid;
            neuronExecutionVals.AfterSigmoidBeforeWeightMultiplicationAtOutputGate = outputGateSigmoidPath;

            outputGateSigmoidPath *= OutputWeight;
            neuronExecutionVals.AfterSigmoidAfterWeightMultiplicationAtOutputGate = outputGateSigmoidPath;

            double outputCellStateTanh = Activation.Tanh(CellState);
            neuronExecutionVals.AfterTanhOutputGate = outputCellStateTanh;

            HiddenState = outputGateSigmoidPath * outputCellStateTanh;

            neuronExecutionVals.OutputHiddenState = HiddenState;
            neuronExecutionVals.OutputCellState = CellState;

            neuronExecutionVals.Activation = HiddenState;

            return HiddenState;
        }

        internal void DeleteMemory()
        {
            HiddenState = 0;
            CellState = 0;
        }

        internal void SubtractGrads(LSTMNeuron gradients, double learningRate)
        {
            Bias -= gradients.Bias * learningRate;
            Weights.SubtractGrads(Weights, learningRate);

            ForgetWeight -= gradients.ForgetWeight * learningRate;
            StoreSigmoidWeight -= gradients.StoreSigmoidWeight * learningRate;
            StoreTanhWeight -= gradients.StoreTanhWeight * learningRate;
            OutputWeight -= gradients.OutputWeight * learningRate;
        }
    }
}
