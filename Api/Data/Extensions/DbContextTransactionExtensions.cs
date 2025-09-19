using Microsoft.EntityFrameworkCore.Storage;

namespace Api.Data.Extensions
{
    public static class DbContextTransactionExtensions
    {
        public static async Task CommitAsync(this IDbContextTransaction transaction, CancellationToken cancellationToken = default)
        {
            if (transaction == null)
                throw new ArgumentNullException(nameof(transaction));
            await transaction.CommitAsync(cancellationToken);
        }

        public static async Task RollbackAsync(this IDbContextTransaction transaction, CancellationToken cancellationToken = default)
        {
            if (transaction == null)
                throw new ArgumentNullException(nameof(transaction));
            await transaction.RollbackAsync(cancellationToken);
        }
    }
}
