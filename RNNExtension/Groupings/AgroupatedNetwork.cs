﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NeatNetwork.Libraries;

namespace NeatNetwork.Groupings
{
    internal class AgroupatedNetwork
    {
        private List<double> input;
        internal List<Connection> Connections;
        public RNN n;

        internal AgroupatedNetwork(RNN n)
        {
            this.n = n;
            Connections = new List<Connection>();
            ClearInput();
        }

        internal void PassInput(double[] wholeInput, Connection connection)
        {
            Range inputRange = connection.InputRange, outputRange = connection.OutputRange;
            for (int networkInputI = inputRange.FromI; networkInputI < inputRange.ToI; networkInputI++)
            {
                for (int inputI = outputRange.FromI; inputI < outputRange.ToI; inputI++)
                {
                    input[networkInputI] += wholeInput[inputI] * connection.Weights[networkInputI - inputRange.FromI][inputI - outputRange.FromI];
                }
            }
        }

        /// <summary>
        /// Connecting to -1 will mean input connected
        /// </summary>
        /// <param name="connectedNetworkI"></param>
        /// <param name="connectedNetworkOutputLength"></param>
        /// <param name="inputRange"></param>
        /// <param name="outputRange"></param>
        /// <param name="maxWeight"></param>
        /// <param name="minWeight"></param>
        /// <param name="weightClosestTo0"></param>
        internal void Connect(int connectedNetworkI, int connectedNetworkOutputLength, Range inputRange, Range outputRange, double maxWeight, double minWeight, double weightClosestTo0)
        {
            var connection = new Connection(inputRange, connectedNetworkI, outputRange, n.InputLength, connectedNetworkOutputLength, maxWeight, minWeight, weightClosestTo0);
            for (int i = 0; i < Connections.Count; i++)
            {
                if (Connections[i].ConnectedNetworkI >= connectedNetworkI)
                {
                    Connections.Insert(i, connection);
                    return;
                }
            }
            Connections.Add(connection);
        }

        internal void ClearInput() => input = new List<double>(new double[n.InputLength]);

        // TODO: implement optimized search
        public bool IsConnectedTo(int networkI)
        {
            for (int i = 0; i < Connections.Count; i++)
                if (Connections[i].ConnectedNetworkI == networkI)
                    return true;

            return false;
        }

        // TODO implement optimized search
        internal Connection GetConnectionConnectedTo(int networkI)
        {
            for (int i = 0; i < Connections.Count; i++)
                if (Connections[i].ConnectedNetworkI == networkI)
                    return Connections[i];

            throw new ArgumentException("Connection doesn't exists");
        }

        internal Range GetOutputRange(int connectionIndex) => FormatRange(Connections[connectionIndex].OutputRange, false);

        internal Range GetInputRange(int connectionIndex) => FormatRange(Connections[connectionIndex].InputRange, true);

        internal Range FormatRange(Range range, bool isInputRange)
        {
            if (range != Range.WholeRange)
            {
                return range;
            }
            else
            {
                int toI = n.InputLength * Convert.ToInt32(isInputRange) + n.OutputLength * Convert.ToInt32(!isInputRange);
                return new Range(range.FromI, toI);
            }
        }
    }
}
