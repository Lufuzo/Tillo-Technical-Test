using Service_Layer.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Tillo_Technical_Test_Solution.ViewModels;

namespace Tillo_Technical_Test_Solution.Controller
{

    
    [RoutePrefix("api/transactions")]
    public class TransactionsController : ApiController
    {
        //Institiating service to I can access service methods in a controller
        private readonly ITransactionService _service;
        public TransactionsController(ITransactionService service)
        {
            _service = service;
        }


        [HttpGet]
        [Route("history")]
        public IHttpActionResult GetHistory(int top = 500)
        {
            var history = _service.History(top);
            return Ok(history);
        }

        [HttpGet]
        [Route("balance")]
        public IHttpActionResult GetBalance()
        {
            var balance = _service.GetBalance();
            return Ok(balance);
        }

        //Perform Deposit
        [HttpPost]
        [Route("deposit")]
        public IHttpActionResult Deposit([FromBody] DepositViewModel dep)
        {
            //Testing for null values from a form 
            if (dep == null || dep.Amount <= 0)
                return BadRequest("Invalid deposit request.");

            var result = _service.Deposit(dep.Amount, dep.Description);
            return Ok(result);
        }
        //Perfom withdrawal
        [HttpPost]
        [Route("withdraw")]
        public IHttpActionResult Withdraw([FromBody] WithdrawViewModel withdr)
        {
            //Testing for null values from a form 
            if (withdr == null || withdr.Amount <= 0)
                return BadRequest("Invalid withdraw request.");

            var result = _service.Withdraw(withdr.Amount, withdr.Description);
            if (!result.Success)
                return BadRequest(result.Message);

            return Ok(result);
        }

        
        //Perform transfer
        [HttpPost]
        [Route("transfer")]
        public IHttpActionResult Transfer([FromBody] TransferViewModel transf)
        {

            //Testing for null values from a form 
            if (transf == null || transf.Amount <= 0 || string.IsNullOrWhiteSpace(transf.Receiver))
                return BadRequest("Invalid transfer request.");

            var result = _service.Transfer(transf.Amount, transf.Receiver, transf.Description);
            if (!result.Success)
                return BadRequest(result.Message);

            return Ok(result);
        }

        private IHttpActionResult BadRequest(object message)
        {
            throw new NotImplementedException();
        }
    }
}
