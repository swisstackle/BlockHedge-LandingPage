using System.Threading.Tasks;
using Microsoft.JSInterop;
using Nethereum.Web3;
using Nethereum.Hex.HexTypes;
using BlockHedge.Services;
using Nethereum.Blazor;
using Nethereum.UI;

namespace BlockHedge.Services
{
    public class WalletService
    {
        private readonly IJSRuntime _jsRuntime;
        private string _selectedAccount;
        private readonly VotingContractService _votingContractService;
        private IWeb3 _web3;
        private readonly IEthereumHostProvider _ethereumHostProvider;

        public event Action<string> AccountChanged;

        // Static field to hold the current instance
        private static WalletService _instance;

        public WalletService(IJSRuntime jsRuntime, VotingContractService votingContractService, IEthereumHostProvider ethereumHostProvider)
        {
            _jsRuntime = jsRuntime;
            _votingContractService = votingContractService;
            _ethereumHostProvider = ethereumHostProvider;
            _instance = this;
        }

        public async Task<bool> CheckIfWalletIsConnected()
        {
            var selectedAccount = await _ethereumHostProvider.GetProviderSelectedAccountAsync();
            if (!string.IsNullOrEmpty(selectedAccount))
            {
                _selectedAccount = selectedAccount;
                _web3 = await _ethereumHostProvider.GetWeb3Async();
                return true;
            }
            return false;
        }

        public async Task<string> ConnectWallet()
        {
            try
            {
                var selectedAccount = await _ethereumHostProvider.EnableProviderAsync();
                if (!string.IsNullOrEmpty(selectedAccount))
                {
                    _selectedAccount = selectedAccount;
                    _web3 = await _ethereumHostProvider.GetWeb3Async();
                    return _selectedAccount;
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error connecting wallet: {ex.Message}");
                return null;
            }
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

        public async Task<string> CreateProposal(string title, string description)
        {
            if (string.IsNullOrEmpty(_selectedAccount))
            {
                throw new InvalidOperationException("Wallet not connected");
            }

            return await _votingContractService.CreateNewProposal(_web3, _selectedAccount, title, description);
        }

        public async Task<string> VoteYes(int proposalIndex)
        {
            if (string.IsNullOrEmpty(_selectedAccount))
            {
                throw new InvalidOperationException("Wallet not connected");
            }

            return await _votingContractService.VoteYes(_web3, _selectedAccount, proposalIndex);
        }

        public async Task<string> VoteNo(int proposalIndex)
        {
            if (string.IsNullOrEmpty(_selectedAccount))
            {
                throw new InvalidOperationException("Wallet not connected");
            }

            return await _votingContractService.VoteNo(_web3, _selectedAccount, proposalIndex);
        }

        public async Task<ProposalDTO> GetProposal(int index)
        {
            return await _votingContractService.GetProposal(_web3, index);
        }

        //public void StartListeningForEvents()
        //{
        //    _votingContractService.StartListeningForEvents();
        //}

        //public event EventHandler<NewProposalEventDTO> OnNewProposal
        //{
        //    add { _votingContractService.OnNewProposal += value; }
        //    remove { _votingContractService.OnNewProposal -= value; }
        //}

        //public event EventHandler<VoteCastEventDTO> OnVoteCast
        //{
        //    add { _votingContractService.OnVoteCast += value; }
        //    remove { _votingContractService.OnVoteCast -= value; }
        //}
    }
}
