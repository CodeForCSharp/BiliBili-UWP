using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml.Data;

namespace bilibili2.Class
{
    public class IncrementalLoadingCollection<T> : ObservableCollection<T>, ISupportIncrementalLoading
    {
        public int CurrentPage { get; set; } = 1;
        public int MaxPage { get; set; } = 0;
        public delegate void LoadingHandler();
        public delegate void ExceptionHandler(Exception e);
        public event LoadingHandler OnLoadingStart;
        public event LoadingHandler OnLoadingEnd;
        public event ExceptionHandler OnError;
        private bool isLoading;
        public bool IsLoading
        {
            get { return isLoading; }
            set
            {
                isLoading = value;
                if(isLoading)
                {
                    OnLoadingStart?.Invoke();
                }
                else
                {
                    OnLoadingEnd?.Invoke();
                }
            }
        }
        public bool HasMoreItems { get; private set; } = true;
        public Func<Task<Tuple<IEnumerable<T>, bool>>> LoadDataTask { get; set; }
        public async Task<LoadMoreItemsResult> LoadMoreItemsAsync(CancellationToken c, uint count)
        {
            if (IsLoading)
            {
                throw new InvalidOperationException("Only one operation in flight at a time");
            }
            isLoading = true;
            try
            {
                var result = await LoadDataTask();
                CurrentPage++;
                var items = result.Item1;
                if (items != null)
                {
                    foreach (T item in items)
                    {
                        Add(item);
                    }
                }
                HasMoreItems = result.Item2;
                return new LoadMoreItemsResult { Count = items == null ? 0 : (uint)items.Count() };
            }
            catch(Exception e)
            {
                OnError?.Invoke(e);
                return new LoadMoreItemsResult { Count = 0 };
            }
            finally
            {
                IsLoading = false;
            }
        }

        public IAsyncOperation<LoadMoreItemsResult> LoadMoreItemsAsync(uint count)
        {
            return AsyncInfo.Run(c => LoadMoreItemsAsync(c, count));
        }

        public void Reset()
        {
            CurrentPage = 1;
            MaxPage = 0;
            ClearItems();
            IsLoading = false;
            HasMoreItems = true;
        }
    }
}
