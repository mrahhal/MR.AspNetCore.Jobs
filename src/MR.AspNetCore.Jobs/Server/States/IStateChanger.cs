using System.Threading.Tasks;
using MR.AspNetCore.Jobs.Models;

namespace MR.AspNetCore.Jobs.Server.States
{
	public interface IStateChanger
	{
		void ChangeState(Job job, IState state, IStorageTransaction transaction);
	}

	public static class StateChangerExtensions
	{
		public static async Task ChangeStateAsync(this IStateChanger @this, Job job, IState state, IStorageConnection connection)
		{
			using (var transaction = connection.CreateTransaction())
			{
				@this.ChangeState(job, state, transaction);
				await transaction.CommitAsync();
			}
		}
	}
}
