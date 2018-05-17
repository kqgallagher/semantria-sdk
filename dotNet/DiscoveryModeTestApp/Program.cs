﻿using System;
using System.IO;
using System.Threading;
using System.Collections.Generic;

using Semantria.Com;
using Semantria.Com.Mapping;
using Semantria.Com.Mapping.Output;

namespace DiscoveryModeTestApp
{
    class Program
    {
        static void Main(string[] args)
        {
            // Set environment vars before calling this program
            // or edit this file and put your key and secret here.
            string consumerKey = Environment.GetEnvironmentVariable("SEMANTRIA_KEY");
            string consumerSecret = Environment.GetEnvironmentVariable("SEMANTRIA_SECRET");

            Collection collection = new Collection() { Id = Guid.NewGuid().ToString(), Documents = new List<string>() };

            Console.WriteLine("Semantria Discovery processing mode demo.");
            Console.WriteLine();

            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "source.txt");
            if (!File.Exists(path))
            {
                Console.WriteLine("Source file isn't available.");
                return;
            }

            // Read collection from the source file
            Console.WriteLine("Reading collection from file...");
            using (StreamReader stream = new StreamReader(path))
            {
                while (!stream.EndOfStream)
                {
                    string line = stream.ReadLine();
                    if (string.IsNullOrEmpty(line) || line.Length < 3)
                        continue;

                    collection.Documents.Add(line);
                }
            }

            CollAnalyticData result = null;

            using (Session session = Semantria.Com.Session.CreateSession(consumerKey, consumerSecret))
            {
                // Error callback handler. This event will occur in case of server-side error
                session.Error += new Session.ErrorHandler(delegate(object sender, ResponseErrorEventArgs ea)
                {
                    Console.WriteLine(string.Format("{0}: {1}", (int)ea.Status, ea.Message));
                });

                //Queues collection for analysis using default configuration
                if (session.QueueCollection(collection) != -1)
                    Console.WriteLine(string.Format("\"{0}\" collection queued successfully.", collection.Id));

                do
                {
                    Thread.Sleep(1000);
                    Console.WriteLine("Retrieving your processed results...");
                    //Retreives analysis results for queued collection
                    result = session.GetCollection(collection.Id);
                }
                while (result.Status == TaskStatus.QUEUED);
            }

            // Print sample of analysis results. (There's lots more in there!)
            Console.WriteLine();
            Console.WriteLine("Facets and attributes:");
            foreach (Facet facet in result.Facets)
            {
                Console.WriteLine("{0} : {1}", facet.Label, facet.Count);
                if (facet.Attributes == null)
                    continue;

                foreach (Semantria.Com.Mapping.Output.Attribute attr in facet.Attributes)
                    Console.WriteLine("\t{0} : {1}", attr.Label, attr.Count);
            }

            Console.WriteLine("Hit any key to exit.");
            Console.ReadKey(false);
        }
    }
}
