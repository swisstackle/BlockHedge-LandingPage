using System;
using System.Numerics;
using System.Threading.Tasks;
using Nethereum.Web3;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;
using Nethereum.Hex.HexTypes;

namespace BlockHedge.Services
{
    public class VotingContractService
    {
        private readonly Web3 _web3;
        private readonly string _contractAddress;
        private readonly Contract _contract;

        public VotingContractService(string rpcUrl, string contractAddress)
        {
            _web3 = new Web3(rpcUrl);
            _contractAddress = contractAddress;
            _contract = _web3.Eth.GetContract(ABI, _contractAddress);
        }

        public async Task<string> CreateNewProposal(string fromAddress, string title, string description)
        {
            var createNewProposalFunction = _contract.GetFunction("createNewProposal");
            var gas = await createNewProposalFunction.EstimateGasAsync(fromAddress, null, null, title, description);
            var receipt = await createNewProposalFunction.SendTransactionAndWaitForReceiptAsync(fromAddress, gas, null, null, title, description);
            return receipt.TransactionHash;
        }

        public async Task<string> VoteYes(string fromAddress, BigInteger index)
        {
            var voteFunction = _contract.GetFunction("vote");
            var gas = await voteFunction.EstimateGasAsync(fromAddress, null, null, index);
            var receipt = await voteFunction.SendTransactionAndWaitForReceiptAsync(fromAddress, gas, null, null, index);
            return receipt.TransactionHash;
        }

        public async Task<string> VoteNo(string fromAddress, BigInteger index)
        {
            var voteNoFunction = _contract.GetFunction("voteNo");
            var gas = await voteNoFunction.EstimateGasAsync(fromAddress, null, null, index);
            var receipt = await voteNoFunction.SendTransactionAndWaitForReceiptAsync(fromAddress, gas, null, null, index);
            return receipt.TransactionHash;
        }

        public async Task<ProposalDTO> GetProposal(BigInteger index)
        {
            var proposalFunction = _contract.GetFunction("proposals");
            var result = await proposalFunction.CallDeserializingToObjectAsync<ProposalDTO>(index);
            return result;
        }

        public event EventHandler<NewProposalEventDTO> OnNewProposal;
        public event EventHandler<VoteCastEventDTO> OnVoteCast;

        //public void StartListeningForEvents()
        //{
        //    var newProposalEvent = _contract.GetEvent("NewProposal");
        //    var voteCastEvent = _contract.GetEvent("VoteCast");

        //    var newProposalFilter = newProposalEvent.CreateFilterAsync().Result;
        //    var voteCastFilter = voteCastEvent.CreateFilterAsync().Result;

        //    _web3.Eth.Filters.Poll(newProposalFilter, (log) =>
        //    {
        //        var decoded = newProposalEvent.DecodeEvent<NewProposalEventDTO>(log);
        //        OnNewProposal?.Invoke(this, decoded.Event);
        //    });

        //    _web3.Eth.Filters.Poll(voteCastFilter, (log) =>
        //    {
        //        var decoded = voteCastEvent.DecodeEvent<VoteCastEventDTO>(log);
        //        OnVoteCast?.Invoke(this, decoded.Event);
        //    });
        //}

        private const string ABI = @"[{""inputs"":[{""internalType"":""string"",""name"":""_title"",""type"":""string""},{""internalType"":""string"",""name"":""_description"",""type"":""string""}],""name"":""createNewProposal"",""outputs"":[],""stateMutability"":""nonpayable"",""type"":""function""},{""inputs"":[{""internalType"":""uint256"",""name"":"""",""type"":""uint256""}],""name"":""proposals"",""outputs"":[{""internalType"":""string"",""name"":""title"",""type"":""string""},{""internalType"":""string"",""name"":""description"",""type"":""string""},{""internalType"":""uint256"",""name"":""yesVotes"",""type"":""uint256""},{""internalType"":""uint256"",""name"":""noVotes"",""type"":""uint256""}],""stateMutability"":""view"",""type"":""function""},{""inputs"":[{""internalType"":""uint256"",""name"":""index"",""type"":""uint256""}],""name"":""vote"",""outputs"":[],""stateMutability"":""nonpayable"",""type"":""function""},{""inputs"":[{""internalType"":""uint256"",""name"":""index"",""type"":""uint256""}],""name"":""voteNo"",""outputs"":[],""stateMutability"":""nonpayable"",""type"":""function""},{""anonymous"":false,""inputs"":[{""indexed"":false,""internalType"":""string"",""name"":""title"",""type"":""string""},{""indexed"":false,""internalType"":""string"",""name"":""description"",""type"":""string""}],""name"":""NewProposal"",""type"":""event""},{""anonymous"":false,""inputs"":[{""indexed"":true,""internalType"":""uint256"",""name"":""indexed_proposalId"",""type"":""uint256""}],""name"":""VoteCast"",""type"":""event""}]";
    }

    [FunctionOutput]
    public class ProposalDTO
    {
        [Parameter("string", "title", 1)]
        public string Title { get; set; }

        [Parameter("string", "description", 2)]
        public string Description { get; set; }

        [Parameter("uint256", "yesVotes", 3)]
        public BigInteger YesVotes { get; set; }

        [Parameter("uint256", "noVotes", 4)]
        public BigInteger NoVotes { get; set; }
    }

    public class NewProposalEventDTO
    {
        [Parameter("string", "title", 1, false)]
        public string Title { get; set; }

        [Parameter("string", "description", 2, false)]
        public string Description { get; set; }
    }

    public class VoteCastEventDTO
    {
        [Parameter("uint256", "indexed_proposalId", 1, true)]
        public BigInteger ProposalId { get; set; }
    }
}
