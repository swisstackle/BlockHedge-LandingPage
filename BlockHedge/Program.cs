using BlockHedge;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using BlockHedge.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddScoped<BlockHedge.Services.WalletService>();

// Register WalletService and VotingContractService
builder.Services.AddSingleton<VotingContractService>(sp => 
    new VotingContractService(
        "https://polygon-rpc.com", // Polygon mainnet RPC URL
        "0x8491C5B2385b86fDdD871F0f983b525D5ddB2C20" // Contract address from contracts.txt
    )
);

await builder.Build().RunAsync();
