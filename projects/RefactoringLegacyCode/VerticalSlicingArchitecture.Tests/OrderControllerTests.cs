using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using FsCheck;

namespace RefactoringLegacyCode.Tests
{
    public class OrderControllerTests
    {
        [Test]
        public void asd()
        {
            new OrdersController().ProcessOrder(1);
            true.Should().BeTrue();
        }
    }

    public class PriorityCalculator
    {

        public static int CalculatePriority(OrderDetails orderDetails)
        {
            //Calculate order priority
            int pr = 0;
            if (orderDetails.Quantity > 10)
            {
               
                // Large orders
                if (orderDetails.DeliveryType == "Express")
                {
                    if (DateTime.Now.Hour >= 18)
                    {
                        pr += 150; // Large express order placed in the evening
                    }
                    else
                    {
                        pr += 120; // Large express order during the day
                    }
                }
                else if (orderDetails.DeliveryType == "SameDay")
                {
                    if (DateTime.Now.Hour < 12)
                    {
                        pr += 180; // SameDay delivery placed in the morning
                    }
                    else
                    {
                        pr += 160; // SameDay delivery placed in the afternoon
                    }
                }
                else if (orderDetails.DeliveryType == "Standard")
                {
                    if (orderDetails.Quantity > 50)
                    {
                        pr += 100; // Very large standard delivery
                    }
                    else
                    {
                        pr += 80; // Regular large standard delivery
                    }
                }
            }
            else
            {
                // Small orders
                if (orderDetails.DeliveryType == "Express")
                {
                    if (orderDetails.Quantity > 5)
                    {
                        if (DateTime.Now.Hour >= 18)
                        {
                            pr += 70; // Small express order placed in the evening
                        }
                        else
                        {
                            pr += 60; // Small express order during the day
                        }
                    }
                    else
                    {
                        pr += 50; // Very small express orders
                    }
                }
                else if (orderDetails.DeliveryType == "SameDay")
                {
                    if (DateTime.Now.Hour < 12)
                    {
                        pr += 90; // Small SameDay delivery placed in the morning
                    }
                    else
                    {
                        pr += 110; // Small SameDay delivery placed later in the day
                    }
                }
                else if (orderDetails.DeliveryType == "Standard")
                {
                    if (DateTime.Now.Hour >= 18)
                    {
                        pr += 40; // Small standard order placed in the evening
                    }
                    else
                    {
                        pr += 20; // Small standard order during the day
                    }
                }
            }

            return pr;
        }
    }

    public class PriorityPropTests
    {
        //https://www.production-ready.de/2023/06/10/property-based-testing-in-csharp-en.html

        public static Arbitrary<string> DeliveryTypes()
        {
            var deliveryTypes = new[] { "Standard", "Express", "SameDay" };

            return Arb.From(Gen.Elements(deliveryTypes));
        }

        public static Arbitrary<int> QuantityBetween1And100()
        {
            return Arb.From(Gen.Choose(1, 100)); 
        }

        [Test]
        public void HigherQuantity_ShouldIncreasePriority()
        {
            Configuration.Default.MaxNbOfTest = 100;

            Prop.ForAll(
                DeliveryTypes(),
                QuantityBetween1And100(), 
                (deliveryType, quantity) =>
                {
                    var lowerQuantityPriority = PriorityCalculator.CalculatePriority(new OrderDetails
                    {
                        Quantity = quantity,
                        DeliveryType = deliveryType
                    });

                    var higherQuantityPriority = PriorityCalculator.CalculatePriority(new OrderDetails
                    {
                        Quantity = quantity + 1,
                        DeliveryType = deliveryType
                    });

                    return lowerQuantityPriority < higherQuantityPriority; 
                }
            ).VerboseCheckThrowOnFailure();
        }

        [Test]
        public void PriorityOrderShouldBeSameDayThenExpresThenStandard()
        {
            Configuration.Default.MaxNbOfTest = 100;

            Prop.ForAll(
                QuantityBetween1And100(),
                (quantity) =>
                {
                    var sameDayPriority = PriorityCalculator.CalculatePriority(new OrderDetails
                    {
                        Quantity = quantity,
                        DeliveryType = "SameDay"
                    });

                    var expressPriority = PriorityCalculator.CalculatePriority(new OrderDetails
                    {
                        Quantity = quantity,
                        DeliveryType = "Express"
                    });

                    var standardPriority = PriorityCalculator.CalculatePriority(new OrderDetails
                    {
                        Quantity = quantity,
                        DeliveryType = "Standard"
                    });

                    return sameDayPriority > expressPriority && expressPriority > standardPriority;
                }
            ).VerboseCheckThrowOnFailure();
        }
    }
}
