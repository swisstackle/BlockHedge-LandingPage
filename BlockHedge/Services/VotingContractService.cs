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
        private readonly string _contractAddress;
        private readonly string _abi;

        public VotingContractService(string contractAddress)
        {
            _contractAddress = contractAddress;
            _abi = @"[{""inputs"":[{""internalType"":""string"",""name"":""_title"",""type"":""string""},{""internalType"":""string"",""name"":""_description"",""type"":""string""}],""name"":""createNewProposal"",""outputs"":[],""stateMutability"":""nonpayable"",""type"":""function""},{""inputs"":[{""internalType"":""uint256"",""name"":"""",""type"":""uint256""}],""name"":""proposals"",""outputs"":[{""internalType"":""string"",""name"":""title"",""type"":""string""},{""internalType"":""string"",""name"":""description"",""type"":""string""},{""internalType"":""uint256"",""name"":""yesVotes"",""type"":""uint256""},{""internalType"":""uint256"",""name"":""noVotes"",""type"":""uint256""}],""stateMutability"":""view"",""type"":""function""},{""inputs"":[{""internalType"":""uint256"",""name"":""index"",""type"":""uint256""}],""name"":""vote"",""outputs"":[],""stateMutability"":""nonpayable"",""type"":""function""},{""inputs"":[{""internalType"":""uint256"",""name"":""index"",""type"":""uint256""}],""name"":""voteNo"",""outputs"":[],""stateMutability"":""nonpayable"",""type"":""function""}]";
        }

        public async Task<string> CreateNewProposal(IWeb3 web3, string fromAddress, string title, string description)
        {
            var contract = web3.Eth.GetContract(_abi, _contractAddress);
            var createNewProposalFunction = contract.GetFunction("createNewProposal");
            var gas = await createNewProposalFunction.EstimateGasAsync(fromAddress, null, null, title, description);
            var receipt = await createNewProposalFunction.SendTransactionAndWaitForReceiptAsync(fromAddress, gas, null, null, title, description);
            return receipt.TransactionHash;
        }

        public async Task<string> VoteYes(IWeb3 web3, string fromAddress, BigInteger index)
        {
            var contract = web3.Eth.GetContract(_abi, _contractAddress);
            var voteFunction = contract.GetFunction("vote");
            var gas = await voteFunction.EstimateGasAsync(fromAddress, null, null, index);
            var receipt = await voteFunction.SendTransactionAndWaitForReceiptAsync(fromAddress, gas, null, null, index);
            return receipt.TransactionHash;
        }

        public async Task<string> VoteNo(IWeb3 web3, string fromAddress, BigInteger index)
        {
            var contract = web3.Eth.GetContract(_abi, _contractAddress);
            var voteNoFunction = contract.GetFunction("voteNo");
            var gas = await voteNoFunction.EstimateGasAsync(fromAddress, null, null, index);
            var receipt = await voteNoFunction.SendTransactionAndWaitForReceiptAsync(fromAddress, gas, null, null, index);
            return receipt.TransactionHash;
        }

        public async Task<ProposalDTO> GetProposal(IWeb3 web3, BigInteger index)
        {
            var contract = web3.Eth.GetContract(_abi, _contractAddress);
            var proposalFunction = contract.GetFunction("proposals");
            var result = await proposalFunction.CallDeserializingToObjectAsync<ProposalDTO>(index);
            return result;
        }
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

        public BigInteger TotalVotes => YesVotes + NoVotes;

        public int YesPercentage => TotalVotes == 0 ? 0 : (int)((YesVotes * 100) / TotalVotes);

        public int NoPercentage => TotalVotes == 0 ? 0 : (int)((NoVotes * 100) / TotalVotes);
    }
}
