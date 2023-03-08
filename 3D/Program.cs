using Utils;

Console.WriteLine("Begin k-means demo");

var rawData = Helper.InitializeRawData3D();

Helper.ShowData(rawData);

int numClusters = 3;
Console.WriteLine("Setting numClusters to " + numClusters);

int[] clustering = Helper.Cluster(rawData, numClusters);

Console.WriteLine("K-means clustering complete");

Console.WriteLine("Final clustering in internal form:");
Helper.ShowVector(clustering, true);

Console.WriteLine("Raw data by cluster:");
Helper.ShowClustered(rawData, clustering, numClusters, 1);

Console.WriteLine("End k-means clustering demo");
