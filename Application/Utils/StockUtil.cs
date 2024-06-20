using Domain.Entities.Models.Clients;

namespace Application.Utils
{
    public static class StockUtil
    {
        public static Tuple<ProductStocks, ProductStockHistorical> CalculateProductStockMinVolume(ProductStocks stocksNow, double newVolumeMin)
        {
            var volumeStock = (stocksNow.Stock * stocksNow.Volume) + stocksNow.VolumeRemaining;
            var stockBefore = stocksNow.Stock;
            var volumeNow = volumeStock - newVolumeMin;
            var newVolumeRemaining = volumeNow % stocksNow.Volume;
            var newStockCalc = Math.Floor(volumeNow / stocksNow.Volume);
            var stockMin = Math.Floor(newVolumeMin / stocksNow.Volume);
            stocksNow.Stock = newStockCalc;
            stocksNow.VolumeRemaining = newVolumeRemaining;

            var newProductHistorical = new ProductStockHistorical()
            {
                ProductId = stocksNow.ProductId,
                Stock = stockMin,
                StockAfter = newStockCalc,
                StockBefore = stockBefore,
                VolumeRemaining = newVolumeRemaining,
            };
            return Tuple.Create(stocksNow, newProductHistorical);
        }
        public static Tuple<ProductStocks, ProductStockHistorical> CalculateProductStockMin(ProductStocks stocksNow, double stock)
        {
            var volumeStock = (stocksNow.Stock * stocksNow.Volume) + stocksNow.VolumeRemaining;
            var stockBefore = stocksNow.Stock;
            var newVolumeMin = stock * stocksNow.Volume;
            var volumeNow = volumeStock - newVolumeMin;
            var newVolumeRemaining = volumeNow % stocksNow.Volume;
            var newStockCalc = Math.Floor(volumeNow / stocksNow.Volume);
            var stockMin = Math.Floor(newVolumeMin / stocksNow.Volume);
            stocksNow.Stock = newStockCalc;
            stocksNow.VolumeRemaining = newVolumeRemaining;

            var newProductHistorical = new ProductStockHistorical()
            {
                ProductId = stocksNow.ProductId,
                Stock = stockMin,
                StockAfter = newStockCalc,
                StockBefore = stockBefore,
                VolumeRemaining = newVolumeRemaining,
            };
            return Tuple.Create(stocksNow, newProductHistorical);
        }
    }
}
