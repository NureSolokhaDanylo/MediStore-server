using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Results.Base
{
	public sealed class Result<T> : Result
	{
		public T? Value { get; private set; }

		private Result(bool isSucceed, T? value, string errorMessage) : base(isSucceed, errorMessage)
		{
			Value = value;
		}

		public static Result<T> Success(T value)
		{
			return new Result<T>(true, value, string.Empty);
		}
		public static Result<T> Failure(string errorMessage)
		{
			return new Result<T>(false, default, errorMessage);
		}
	}
}
