﻿using VerticalSlicingArchitecture.Shared;

namespace VerticalSlicingArchitecture.Entities
{
    public enum LastOperation
    {
        None,
        Picked,
        Unpicked
    }

    public class Product
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public decimal Price { get; set; }
        public LastOperation LastOperation { get; set; }

        public Product()
        {
            Id = Guid.NewGuid();
        }

        public StockLevel StockLevel { get; set; }

        public Result Pick(int pickCount)
        {
            LastOperation = LastOperation.Picked;

            return StockLevel.Decrease(pickCount);
        }

        public void Unpick(int quantity)
        {
            LastOperation = LastOperation.Unpicked;

            StockLevel.Increase(quantity);
        }
    }
}
