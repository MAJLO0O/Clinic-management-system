using MedicalData.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGenerator.Generators
{
    public  class PaymentGenerator
    {
        public List<Payment> GeneratePayments(List<int> appointmentWithoutPaymentIds,List<int>methodIds,List<int>statusIds,HashSet<int> usedNumbers)
        {
         
            int paymentNumber;
            decimal amount;
            int appointmentId;
            int? paymentMethodId;
            int statusId;
            List<Payment> result = new();

            for (int i=0;i< appointmentWithoutPaymentIds.Count; i++)
                {
            do
            {
                paymentNumber = Random.Shared.Next(1000000, 100000000);

            }while (usedNumbers.Contains(paymentNumber));
                usedNumbers.Add(paymentNumber);
             amount = Random.Shared.Next(100, 1000);
                appointmentId = appointmentWithoutPaymentIds[Random.Shared.Next(appointmentWithoutPaymentIds.Count)];
                appointmentWithoutPaymentIds.Remove(appointmentId);  
                statusId = statusIds[Random.Shared.Next(statusIds.Count)];
                if (statusId == statusIds[1])
                {
                    paymentMethodId = null;
                }
                else
                {
                    paymentMethodId = methodIds[Random.Shared.Next(methodIds.Count)];
                }


                result.Add(new Payment
                    {
                        PaymentNumber = paymentNumber,
                        Amount = amount,
                        AppointmentId = appointmentId,
                        PaymentMethodId = paymentMethodId,
                        StatusId = statusId
                    });
                }
                return result;

        }
    }
}
