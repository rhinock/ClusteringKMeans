namespace Utils
{
    public static class Helper
    {
        public static double[][] InitializeRawData2D()
        {
            var rawData = new double[20][];

            rawData[0] = new double[] { 65.0, 220.0 };
            rawData[1] = new double[] { 73.0, 160.0 };
            rawData[2] = new double[] { 59.0, 110.0 };
            rawData[3] = new double[] { 61.0, 120.0 };
            rawData[4] = new double[] { 75.0, 150.0 };
            rawData[5] = new double[] { 67.0, 240.0 };
            rawData[6] = new double[] { 68.0, 230.0 };
            rawData[7] = new double[] { 70.0, 220.0 };
            rawData[8] = new double[] { 62.0, 130.0 };
            rawData[9] = new double[] { 66.0, 210.0 };
            rawData[10] = new double[] { 77.0, 190.0 };
            rawData[11] = new double[] { 75.0, 180.0 };
            rawData[12] = new double[] { 74.0, 170.0 };
            rawData[13] = new double[] { 70.0, 210.0 };
            rawData[14] = new double[] { 61.0, 110.0 };
            rawData[15] = new double[] { 58.0, 100.0 };
            rawData[16] = new double[] { 66.0, 230.0 };
            rawData[17] = new double[] { 59.0, 120.0 };
            rawData[18] = new double[] { 68.0, 210.0 };
            rawData[19] = new double[] { 61.0, 130.0 };

            return rawData;
        }

        public static double[][] InitializeRawData3D()
        {
            const int X = 9;
            const int Y = 9;
            const int dimensions = 3;
            double[] xAxis = new double[X] { -2, -1.5, -1, -0.5, 0, 0.5, 1, 1.5, 2 };
            double[] yAxis = new double[Y] { -2, -1.5, -1, -0.5, 0, 0.5, 1, 1.5, 2 };
            double[][] zAxis = new double[X][];
            double[][] rawData = new double[X * Y][];

            Console.WriteLine("Begin k-means demo");

            for (int x = 0; x < xAxis.Length; x++)
            {
                zAxis[x] = new double[yAxis.Length];

                for (int y = 0; y < yAxis.Length; y++)
                {
                    zAxis[x][y] = 5 * Math.Pow(xAxis[y], 2) - 4 * Math.Pow(yAxis[x], 4);
                    rawData[x * Y + y] = new double[dimensions] { xAxis[y], yAxis[x], zAxis[x][y] };

                    // Console.Write($"{zAxis[x][y]}\t");
                }

                // Console.WriteLine();
            }

            return rawData;
        }

        public static int[] Cluster(double[][] rawData, int numClusters)
        {
            double[][] data = Normalized(rawData);
            bool changed = true; bool success = true;
            int[] clustering = InitClustering(data.Length, numClusters, 0);
            double[][] means = Allocate(numClusters, data[0].Length);
            int maxCount = data.Length * 10;
            int ct = 0;
            while (changed == true && success == true && ct < maxCount)
            {
                ++ct;
                success = UpdateMeans(data, clustering, means);
                changed = UpdateClustering(data, clustering, means);
            }
            return clustering;
        }

        public static double[][] Normalized(double[][] rawData)
        {
            double[][] result = new double[rawData.Length][];
            for (int i = 0; i < rawData.Length; ++i)
            {
                result[i] = new double[rawData[i].Length];
                Array.Copy(rawData[i], result[i], rawData[i].Length);
            }

            for (int j = 0; j < result[0].Length; ++j)
            {
                double colSum = 0.0;
                for (int i = 0; i < result.Length; ++i)
                    colSum += result[i][j];
                double mean = colSum / result.Length;
                double sum = 0.0;
                for (int i = 0; i < result.Length; ++i)
                    sum += (result[i][j] - mean) * (result[i][j] - mean);
                double sd = sum / result.Length;
                for (int i = 0; i < result.Length; ++i)
                    result[i][j] = (result[i][j] - mean) / sd;
            }
            return result;
        }

        static int[] InitClustering(int numTuples, int numClusters, int seed)
        {
            Random random = new Random(seed);
            int[] clustering = new int[numTuples];
            for (int i = 0; i < numClusters; ++i)
                clustering[i] = i;
            for (int i = numClusters; i < clustering.Length; ++i)
                clustering[i] = random.Next(0, numClusters);
            return clustering;
        }

        static bool UpdateMeans(double[][] data, int[] clustering, double[][] means)
        {
            int numClusters = means.Length;
            int[] clusterCounts = new int[numClusters];
            for (int i = 0; i < data.Length; ++i)
            {
                int cluster = clustering[i];
                ++clusterCounts[cluster];
            }

            for (int k = 0; k < numClusters; ++k)
                if (clusterCounts[k] == 0)
                    return false;

            for (int k = 0; k < means.Length; ++k)
                for (int j = 0; j < means[k].Length; ++j)
                    means[k][j] = 0.0;

            for (int i = 0; i < data.Length; ++i)
            {
                int cluster = clustering[i];
                for (int j = 0; j < data[i].Length; ++j)
                    means[cluster][j] += data[i][j]; // accumulate sum
            }

            for (int k = 0; k < means.Length; ++k)
                for (int j = 0; j < means[k].Length; ++j)
                    means[k][j] /= clusterCounts[k]; // danger of div by 0
            return true;
        }

        static double[][] Allocate(int numClusters, int numColumns)
        {
            double[][] result = new double[numClusters][];
            for (int k = 0; k < numClusters; ++k)
                result[k] = new double[numColumns];
            return result;
        }

        static bool UpdateClustering(double[][] data, int[] clustering, double[][] means)
        {
            int numClusters = means.Length;
            bool changed = false;

            int[] newClustering = new int[clustering.Length];
            Array.Copy(clustering, newClustering, clustering.Length);

            double[] distances = new double[numClusters];

            for (int i = 0; i < data.Length; ++i)
            {
                for (int k = 0; k < numClusters; ++k)
                    distances[k] = Distance(data[i], means[k]);

                int newClusterID = MinIndex(distances);
                if (newClusterID != newClustering[i])
                {
                    changed = true;
                    newClustering[i] = newClusterID;
                }
            }

            if (changed == false)
                return false;

            int[] clusterCounts = new int[numClusters];
            for (int i = 0; i < data.Length; ++i)
            {
                int cluster = newClustering[i];
                ++clusterCounts[cluster];
            }

            for (int k = 0; k < numClusters; ++k)
                if (clusterCounts[k] == 0)
                    return false;

            Array.Copy(newClustering, clustering, newClustering.Length);
            return true; // no zero-counts and at least one change
        }

        static double Distance(double[] tuple, double[] mean)
        {
            double sumSquaredDiffs = 0.0;
            for (int j = 0; j < tuple.Length; ++j)
                sumSquaredDiffs += Math.Pow((tuple[j] - mean[j]), 2);
            return Math.Sqrt(sumSquaredDiffs);
        }

        static int MinIndex(double[] distances)
        {
            int indexOfMin = 0;
            double smallDist = distances[0];
            for (int k = 0; k < distances.Length; ++k)
            {
                if (distances[k] < smallDist)
                {
                    smallDist = distances[k];
                    indexOfMin = k;
                }
            }
            return indexOfMin;
        }

        public static void ShowData(double[][] data, int decimals, bool indices, bool newLine)
        {
            for (int i = 0; i < data.Length; ++i)
            {
                if (indices) Console.Write(i.ToString().PadLeft(3) + " ");
                for (int j = 0; j < data[i].Length; ++j)
                {
                    if (data[i][j] >= 0.0) Console.Write(" ");
                    Console.Write(data[i][j].ToString("F" + decimals) + " ");
                }
                Console.WriteLine("");
            }
            if (newLine) Console.WriteLine("");
        }

        public static void ShowData(double[][] data)
        {
            Console.WriteLine("Raw unclustered data:");

            foreach (var row in data)
            {
                foreach (var column in row)
                {
                    Console.Write($"{column}\t");
                }
                Console.WriteLine();
            }
        }

        public static void ShowVector(int[] vector, bool newLine)
        {
            for (int i = 0; i < vector.Length; ++i)
                Console.Write(vector[i] + " ");
            if (newLine) Console.WriteLine("\n");
        }

        public static void ShowClustered(double[][] data, int[] clustering, int numClusters, int decimals)
        {
            for (int k = 0; k < numClusters; ++k)
            {
                Console.WriteLine("===================");
                for (int i = 0; i < data.Length; ++i)
                {
                    int clusterID = clustering[i];
                    if (clusterID != k) continue;
                    Console.Write(i.ToString().PadLeft(3) + " ");
                    for (int j = 0; j < data[i].Length; ++j)
                    {
                        if (data[i][j] >= 0.0) Console.Write(" ");
                        Console.Write(data[i][j].ToString("F" + decimals) + " ");
                    }
                    Console.WriteLine("");
                }
                Console.WriteLine("===================");
            } // k
        }
    }
}