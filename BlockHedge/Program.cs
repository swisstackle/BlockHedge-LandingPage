using BlockHedge;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using BlockHedge.Services;
using Nethereum.UI;
using Nethereum.Metamask;
using Nethereum.Metamask.Blazor;
using Microsoft.AspNetCore.Components.Authorization;
using Nethereum.Blazor;


var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");
builder.Services.AddAuthorizationCore();


// Register IEthereumHostProvider
builder.Services.AddSingleton<IEthereumHostProvider, MetamaskHostProvider>();

builder.Services.AddSingleton(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddSingleton<IMetamaskInterop, MetamaskBlazorInterop>();
builder.Services.AddSingleton<MetamaskInterceptor>();
builder.Services.AddSingleton<MetamaskHostProvider>();

//Add metamask as the selected ethereum host provider
builder.Services.AddSingleton(services =>
{
    var metamaskHostProvider = services.GetService<MetamaskHostProvider>();
    var selectedHostProvider = new SelectedEthereumHostProviderService();
    selectedHostProvider.SetSelectedEthereumHostProvider(metamaskHostProvider);
    return selectedHostProvider;
});


builder.Services.AddSingleton<AuthenticationStateProvider, EthereumAuthenticationStateProvider>();

// Register VotingContractService
builder.Services.AddSingleton<VotingContractService>(sp => 
    new VotingContractService("0x9190428F46c1c6eB765dc7F7169eF3470eccd903")
);


// Register WalletService
builder.Services.AddScoped<WalletService>();

await builder.Build().RunAsync();
