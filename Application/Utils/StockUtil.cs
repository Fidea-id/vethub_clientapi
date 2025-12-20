using Domain.Entities.Models.Clients;

namespace Application.Utils
{
    public static class StockUtil
    {
        public static double SafeParseDouble(double value)
        {
            if (double.IsInfinity(value) || double.IsNaN(value))
                return 0;
            return value;
        }
        public static Tuple<ProductStocks, ProductStockHistorical> CalculateProductStockMinVolume(ProductStocks stocksNow, double newVolumeMin)
        {
            var volumeStock = (stocksNow.Stock * stocksNow.Volume) + stocksNow.VolumeRemaining;
            var stockBefore = stocksNow.Stock;
            var volumeNow = Math.Max(0, volumeStock - newVolumeMin);
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
        public static Tuple<ProductStocks, ProductStockHistorical> CalculateProductStockPlusVolume(ProductStocks stocksNow, double returnVolume)
        {
            if (stocksNow.Volume == 0)
            {
                stocksNow.Volume = 1; // Set default volume to 1 if it's zero
            }
            var volumeStock = (stocksNow.Stock * stocksNow.Volume) + stocksNow.VolumeRemaining;
            var stockBefore = stocksNow.Stock;
            var volumeNow = volumeStock + returnVolume; // Tambahkan kembali stok
            var newVolumeRemaining = volumeNow % stocksNow.Volume;
            var newStockCalc = Math.Floor(volumeNow / stocksNow.Volume);
            var stockPlus = Math.Floor(returnVolume / stocksNow.Volume);

            stocksNow.Stock = newStockCalc;
            stocksNow.VolumeRemaining = newVolumeRemaining;

            var newProductHistorical = new ProductStockHistorical()
            {
                ProductId = stocksNow.ProductId,
                Stock = stockPlus,
                StockAfter = newStockCalc,
                StockBefore = stockBefore,
                VolumeRemaining = newVolumeRemaining,
            };

            return Tuple.Create(stocksNow, newProductHistorical);
        }
        public static Tuple<ProductStocks, ProductStockHistorical> CalculateProductStockUp(ProductStocks stocksNow, double stock)
        {
            var volumeStock = (stocksNow.Stock * stocksNow.Volume) + stocksNow.VolumeRemaining;
            var stockBefore = stocksNow.Stock;
            var newVolumeMin = stock * stocksNow.Volume;
            var volumeNow = volumeStock + newVolumeMin;
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
