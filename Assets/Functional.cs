using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Functional {
	class F {

		static public void ForEach<T> (Action<T> fn, T[] array) {
			foreach (var item in array)
			{
				fn(item);
			}
		}
		static public T1[] Map<T, T1> (Func<T, T1> fn, T[] array) { return array.Select(fn).ToArray(); }
		static public T[] Filter<T> (Func<T, bool> fn, T[] array) { return array.Where(fn).ToArray(); }

		static public T[] Copy<T> (T[] array, int start, int length) {
			T[] dest = new T[array.Length - 1];
			Array.Copy(array, start, dest, 0, length);
			return dest;
		}

		static public T Head<T> (T[] array) {
			return array[0];
		}

		static public T[] Tail<T> (T[] array) {
			return F.Copy(array, 1, array.Length - 1);
		}

		static public B Reduce<T, B> (B baseAcc, Func<B, T, B> fn, T[] array) { return array.Aggregate(baseAcc, fn); }
		static public Func<T1, T> Compose<T1, T> (Func<object, object>[] fnArray) where T:class {
			Func<T1, T> toExecute = (T1 val) => {
				return F.Reduce(val, (object acc, Func<object, object> current) => current(acc), fnArray) as T;
			};

			return toExecute;
		}
	}
}
