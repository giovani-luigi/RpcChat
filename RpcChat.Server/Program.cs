using System;

namespace RpcChat.Server {
    class Program {
        
        const int SERVER_PORT = 555;

        static void Main(string[] args) {

            Console.WriteLine("Starting RPC Chat server...");

            Grpc.Core.Server server = new Grpc.Core.Server() {
                Services = {
                    ChatContract.BindService(new Rpc.ChatService())
                },
                Ports = {
                    new Grpc.Core.ServerPort("localhost", SERVER_PORT, Grpc.Core.ServerCredentials.Insecure)
                }
            };

            try {
                server.Start();
                Console.WriteLine($"Chat service listening in port {SERVER_PORT}");
            } catch (Exception) {
                Console.WriteLine("Failed to start RPC server");
            }

            Console.WriteLine("Press any key to exit the server...");
            Console.ReadKey();

            server.ShutdownAsync().Wait();
        }
    }
}
