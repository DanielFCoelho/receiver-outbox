using DepositOrderCreationCAP.Database;

using DotNetCore.CAP;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using ReceiverOutbox.ViewModels;

namespace ReceiverOutbox.Receivers
{
    public class Receiver : ICapSubscribe
    {
        private readonly DOCreationContext _context;        

        public Receiver(DOCreationContext context)
        {
            _context = context;
        }


        [CapSubscribe("receiveMessage")]
        public async Task ReceivingAndProcessingDepositOrder(DepositOrderCreateEventViewModel message)
        {
            try
            {
                var depositOrder = await _context.DepositOrders.Include(k => k.Transactions).FirstOrDefaultAsync(k => k.Id == message.DepositOrderId);
                if (depositOrder == null)
                {
                    return;
                }

                depositOrder.Process();

                _context.DepositOrders.Update(depositOrder);

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw ex;
            }


        }

        [CapSubscribe("receiveCallBack")]
        public async Task Callback()
        {
            await Console.Out.WriteLineAsync("CALLBACK RECEIVED !!");
        }
    }
}
