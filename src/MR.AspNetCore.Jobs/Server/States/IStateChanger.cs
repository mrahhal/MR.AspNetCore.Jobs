using MR.AspNetCore.Jobs.Models;

namespace MR.AspNetCore.Jobs.Server.States
{
	public interface IStateChanger
	{
		void ChangeState(Job job, IState state, IStorageTransaction transaction);
	}
}
