using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace BlockHedge.Services
{
    public class WalletService
    {
        private readonly IJSRuntime _jsRuntime;
        private string _selectedAccount;

        public event Action<string> AccountChanged;

        // Static field to hold the current instance
        private static WalletService _instance;

        public WalletService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
            _instance = this;
        }

        public async Task<bool> CheckIfWalletIsConnected()
        {
            var accounts = await _jsRuntime.InvokeAsync<string[]>("ethereum.request", new object[] { new { method = "eth_accounts" } });
            if (accounts.Length > 0)
            {
                _selectedAccount = accounts[0];
                return true;
            }
            return false;
        }

        public async Task<string> ConnectWallet()
        {
            var accounts = await _jsRuntime.InvokeAsync<string[]>("ethereum.request", new object[] { new { method = "eth_requestAccounts" } });
            if (accounts.Length > 0)
            {
                _selectedAccount = accounts[0];
                return _selectedAccount;
            }
            return null;
        }

        public string GetSelectedAccount()
        {
            return _selectedAccount;
        }
        [JSInvokable("OnAccountsChanged")]
        public static Task OnAccountsChanged(string[] accounts)
        {
            Console.WriteLine($"Accounts changed: {string.Join(", ", accounts)}");
            _instance?.HandleAccountsChanged(accounts);
            return Task.CompletedTask;
        }

        public void HandleAccountsChanged(string[] accounts)
        {
            if (accounts.Length > 0)
            {
                _selectedAccount = accounts[0];
                AccountChanged?.Invoke(_selectedAccount);
            }
            else
            {
                _selectedAccount = null;
                AccountChanged?.Invoke(null);
            }
        }
    }
}