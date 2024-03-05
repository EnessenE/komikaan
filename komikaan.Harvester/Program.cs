using System;
using GTFS;
using Neo4j.Driver;

namespace komikaan.Harvester
{
    internal class Program
    {
        static async Task Main(string[] args)
        {

            string path = "E:\\gtfs-testing\\gtfs-nl-mini.zip";

            var reader = new GTFSReader<GTFSFeed>();
            var feed = reader.Read(path);

            Console.WriteLine($"Found a feed with {feed.Agencies.Count} agencies");

            var driver = GraphDatabase.Driver("neo4j://localhost:7687", AuthTokens.Basic("neo4j", "s3cr3t"));
            using var session = driver.AsyncSession();
            Console.WriteLine("Connected!");

            // await CreateAgencies(feed, session);
            //
            // await CreateRoutes(feed, session);

            await CreateTrips(feed, session);
            Console.WriteLine("Finished!");
        }

        private static async Task CreateTrips(GTFSFeed feed, IAsyncSession session)
        {
            foreach (var trip in feed.Trips)
            {
                Console.WriteLine($"Trip: {trip.Id}");
                await session.ExecuteWriteAsync(
                    async tx =>
                    {
                        await tx.RunAsync(
                            @"MERGE (item:Trip {id: $id})
ON CREATE SET item.accessibilityType = $accessibilityType
ON MATCH SET item.accessibilityType = $accessibilityType",
                            new { id = trip.Id, accessibilityType = trip.AccessibilityType.ToString()});
                    });


                //     await client.Cypher
                //         .Create("(route:Route {id:$id, type:$type})")
                //         .WithParam("type", feedRoute.Type)
                //         .WithParam("id", feedRoute.Id)
                //         .ExecuteWithoutResultsAsync();
                Console.WriteLine($"Created Trip: {trip.Id}");


                await session.ExecuteWriteAsync(
                    async tx =>
                    {
                        await tx.RunAsync(
                            @"MATCH (trip:Trip {id: $tripId}), (route:Route {id: $routeId})
MERGE (trip)-[r:BELONGS_TO]->(route)
ON CREATE SET r.created = timestamp()
SET r.updated = timestamp()",
                            new { tripId = trip.Id, routeId = trip.RouteId });
                    });


                //     await client.Cypher
                //         .Match("(route:Route)", "(agency:Agency)")
                //         .Where((GraphRoute route) => route.id == feedRoute.Id)
                //         .AndWhere((GraphAgency agency) => agency.id == feedRoute.AgencyId)
                //         .CreateUnique("route-[:RUN_BY]->agency")
                //         .ExecuteWithoutResultsAsync();
                //
                //
                Console.WriteLine($"Created relation: {trip.Id} -> {trip.RouteId}");
            }
        }

        private static async Task CreateAgencies(GTFSFeed feed, IAsyncSession session)
        {
            foreach (var agency in feed.Agencies)
            {
                await session.ExecuteWriteAsync(
                    async tx =>
                    {
                        await tx.RunAsync(
                            @"MERGE (agency:Agency {id: $id})
ON CREATE SET agency.name = $name
ON MATCH SET agency.name = $name",
                            new { id = agency.Id, name = agency.Name });
                    });

                // await client.Cypher
                //     .Create("(agency:Agency {name:$agency, id:$id})")
                //     .WithParam("agency", agency.Name)
                //     .WithParam("id", agency.Id)
                //     .ExecuteWithoutResultsAsync();
                Console.WriteLine($"Created {agency.Name}!");
            }
        }

        private static async Task CreateRoutes(GTFSFeed feed, IAsyncSession session)
        {
            foreach (var feedRoute in feed.Routes)
            {
                Console.WriteLine($"Route: {feedRoute.LongName}");
                await session.ExecuteWriteAsync(
                    async tx =>
                    {
                        await tx.RunAsync(
                            @"MERGE (item:Route {id: $id})
ON CREATE SET item.type = $type
ON MATCH SET item.type = $type",
                            new { id = feedRoute.Id, type = feedRoute.Type.ToString() });
                    });


                //     await client.Cypher
                //         .Create("(route:Route {id:$id, type:$type})")
                //         .WithParam("type", feedRoute.Type)
                //         .WithParam("id", feedRoute.Id)
                //         .ExecuteWithoutResultsAsync();
                Console.WriteLine($"Created route: {feedRoute.LongName}");


                await session.ExecuteWriteAsync(
                    async tx =>
                    {
                        await tx.RunAsync(
                            @"MATCH (agency:Agency {id: $agencyId}), (route:Route {id: $routeId})
MERGE (route)-[r:RUN_BY]->(agency)
ON CREATE SET r.created = timestamp()
SET r.updated = timestamp()",
                            new { agencyId = feedRoute.AgencyId, routeId = feedRoute.Id });
                    });


                //     await client.Cypher
                //         .Match("(route:Route)", "(agency:Agency)")
                //         .Where((GraphRoute route) => route.id == feedRoute.Id)
                //         .AndWhere((GraphAgency agency) => agency.id == feedRoute.AgencyId)
                //         .CreateUnique("route-[:RUN_BY]->agency")
                //         .ExecuteWithoutResultsAsync();
                //
                //
                Console.WriteLine($"Created relation: {feedRoute.AgencyId} {feedRoute.LongName}");
            }
        }
    }
}
