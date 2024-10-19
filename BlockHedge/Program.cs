using BlockHedge;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using BlockHedge.Services;
using Nethereum.UI;
using Nethereum.Metamask;
using Nethereum.Metamask.Blazor;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddSingleton<IMetamaskInterop, MetamaskBlazorInterop>();

// Register VotingContractService
builder.Services.AddSingleton<VotingContractService>(sp => 
    new VotingContractService("0x9190428F46c1c6eB765dc7F7169eF3470eccd903")
);

// Register IEthereumHostProvider
builder.Services.AddSingleton<IEthereumHostProvider, MetamaskHostProvider>();

// Register WalletService
builder.Services.AddScoped<WalletService>();

await builder.Build().RunAsync();
