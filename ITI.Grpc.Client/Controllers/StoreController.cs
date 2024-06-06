using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Net.Client;
using ITI.Grpc.Protos;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using static ITI.Grpc.Protos.InventoryServiceProto;

namespace ITI.Grpc.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StoreController : Controller
    {



        [HttpGet]
        public async Task<ActionResult> AllProducts()
        {
            var apiKey = "AIzaSyD7Q6Q6-4"; // Replace with your actual API key

            // Configure the channel with the endpoint of your gRPC service
            var channel = GrpcChannel.ForAddress("https://localhost:7214", new GrpcChannelOptions
            {
                Credentials = ChannelCredentials.SecureSsl
            });

            // Create call credentials using the API key
            var callCredentials = CallCredentials.FromInterceptor((context, metadata) =>
            {
                metadata.Add("x-api-key", apiKey);
                return Task.CompletedTask;
            });

            // Combine SSL credentials with call credentials
            var compositeCredentials = ChannelCredentials.Create(new SslCredentials(), callCredentials);

            // Create the client using the configured channel and credentials
            var client = new InventoryServiceProto.InventoryServiceProtoClient(channel);

            try
            {
                // Make a gRPC call to your service
                var products = await client.GetAllAsync(new Google.Protobuf.WellKnownTypes.Empty(), new CallOptions(credentials: callCredentials));
                return Ok(products);
            }

            catch (Exception ex)
            {
                // Log other exceptions for debugging
                Console.WriteLine("An error occurred: " + ex.Message);
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }


        [HttpPost]
        public async Task<ActionResult> AddProduct(Product product)
        {
            var apiKey = "AIzaSyD7Q6Q6-4"; // Replace with your actual API key

            // Configure the channel with the endpoint of your gRPC service
            var channel = GrpcChannel.ForAddress("https://localhost:7214", new GrpcChannelOptions
            {
                Credentials = ChannelCredentials.SecureSsl
            });

            // Create call credentials using the API key
            var callCredentials = CallCredentials.FromInterceptor((context, metadata) =>
            {
                metadata.Add("x-api-key", apiKey);
                return Task.CompletedTask;
            });

            // Combine SSL credentials with call credentials
            var compositeCredentials = ChannelCredentials.Create(new SslCredentials(), callCredentials);

            // Create the client using the configured channel and credentials
            var client = new InventoryServiceProto.InventoryServiceProtoClient(channel);

            try
            {
                // Check if the product exists
                var isExisted = await client.GetProductByIdAsync(new Id { Id_ = product.Id }, new CallOptions(credentials: callCredentials));

                if (!isExisted.IsExisted_)
                {
                    // Add the product if it does not exist
                    var addedProduct = await client.AddProductAsync(product, new CallOptions(credentials: callCredentials));
                    return Created("Product Created", addedProduct);
                }

                // Update the product if it exists
                var updatedProduct = await client.UpdateProductAsync(product, new CallOptions(credentials: callCredentials));
                return Created("Product Updated", updatedProduct);
            }
            catch (Exception ex)
            {
                // Log other exceptions for debugging
                Console.WriteLine("An error occurred: " + ex.Message);
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }


        [HttpPost("addproducts")]

        public async Task<ActionResult> AddBulkProducts(List<Product> productToAdds)
        {
            var channel = GrpcChannel.ForAddress("https://localhost:7214");
            var client = new InventoryServiceProtoClient(channel);

            var call = client.AddBulkProducts();

            foreach (var product in productToAdds)
            {
                await call.RequestStream.WriteAsync(product);
                await Task.Delay(1000);
            }

            await call.RequestStream.CompleteAsync();

            var response = await call.ResponseAsync;

            return Ok(response);
        }

        [HttpGet("GetReport")]
        public async Task<ActionResult> GetReport()
        {
            List<Product> productToAdds = new List<Product>();

            var channel = GrpcChannel.ForAddress("https://localhost:7214");
            var client = new InventoryServiceProtoClient(channel);

            var call = client.GetProductReport(new Google.Protobuf.WellKnownTypes.Empty());

            while (await call.ResponseStream.MoveNext(CancellationToken.None))
            {
                productToAdds.Add(call.ResponseStream.Current);
            }

            return Ok(productToAdds);
        }
    }
}
