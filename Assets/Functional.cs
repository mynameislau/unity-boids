using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Functional {
	class F {

		static public IEnumerable<T> Times<T> (int times, Func<int, T> fn) {
			T[] array = new T[times];

			for (int i = 0; i < times; i++)
			{
				array[i] = fn(i);
			}

			return array;
		}

		// static public IEnumerable<T> Reject<T> (Func<T, bool> fn, IEnumerable<T> array) {
		// 	return array.
		// }

		// not functional
		static public void ForEach<T> (Action<T, int> fn, IEnumerable<T> array) {
			int index = 0;
			foreach (var item in array)
			{
				fn(item, index);
				index ++;
			}
		}
		static public void ForEach<T> (Action<T> fn, IEnumerable<T> array) {
			foreach (var item in array)
			{
				fn(item);
			}
		}
		//

		static public IEnumerable<T> FilterOutNulls<T> (IEnumerable<T?> array) where T:struct {
			IEnumerable<T?> filtered = F.Filter((entry) => entry.HasValue, array);
			return F.Map(entry => entry.Value, filtered);
		}

		static public T? Find<T> (Func<T, bool> fn, IEnumerable<T> array) where T:struct {
			try {
				T result = array.First(fn);
				return result;
			}
			catch {
				return null;
			}

		}
		static public T Identity<T> (T val) { return val; }
		static public IEnumerable<T1> Map<T, T1> (Func<T, T1> fn, IEnumerable<T> array) { return array.Select(fn); }
		static public IEnumerable<T> Filter<T> (Func<T, bool> fn, IEnumerable<T> enumerable) { return enumerable.Where(fn); }
		static public List<T> Filter<T> (Func<T, bool> fn, List<T> enumerable) { return enumerable.Where(fn).ToList(); }

		static public IEnumerable<T> Copy<T> (IEnumerable<T> array, int start, int length) {
			return array.Skip(start).Take(length);
		}

		static public T Head<T> (IEnumerable<T> array) {
			return array.First();
		}

		static public IEnumerable<T> Tail<T> (IEnumerable<T> array) {
			return F.Copy(array, 1, array.Count() - 1);
		}

		static public int Length<T> (IEnumerable<T> collection) {
			return collection.Count();
		}

		static public T Nth<T> (int index, IEnumerable<T> collection) {
			return collection.ElementAt(index);
		}

		static public B Reduce<T, B> (B baseAcc, Func<B, T, B> fn, IEnumerable<T> array) { return array.Aggregate(baseAcc, fn); }
		static public Func<T1, T> Compose<T1, T> (Func<object, object>[] fnArray) where T:class {
			Func<T1, T> toExecute = (T1 val) => {
				return F.Reduce(val, (object acc, Func<object, object> current) => current(acc), fnArray) as T;
			};

			return toExecute;
		}

		// static public Func<T> Curry<T1, T> (Func<T1, T> fn) {

		static public Func<T1, T3> Compose<T1, T2, T3> (Func<T2, T3> fn2, Func<T1, T2> fn1) {
			return (a) => fn2(fn1(a));
		}
		static public Func<T1, T4> Compose<T1, T2, T3, T4> (Func<T3, T4> fn3, Func<T2, T3> fn2, Func<T1, T2> fn1) {
			return (a) => fn3(fn2(fn1(a)));
		}
	}
}
