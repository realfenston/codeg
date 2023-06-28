<fim_prefix>﻿using Google.Protobuf;
using System;
using System.Collections.Generic;
namespace PlatformHotfix
{
     public static class SimpleCollectionExt
    {
        public static T Find<T, C>(this SimpleCollection<T> ts, C value, Func<T, C, bool> func)
        {
            for (int i = 0; i < ts.Count; i++)
            {
                if (func(ts[i], value))
                {
                    return ts[i];
                }
            }
            return default(T);
        }

        public static int FindIndex<T, C>(this SimpleCollection<T> list, C ctx, Func<T, C, bool> match)
        {
            for (int i = 0, count = list.Count; i < count; ++i)
            {
                if (match(list[i], ctx))
                {
                    return i;
                }
            }
            return -1;
        }

        public static int FindLastIndex<T, C>(this SimpleCollection<T> list, C ctx, Func<T, C, bool> match)
        {
            for (int i = list.Count - 1; i >= 0; --i)
            {
                if (match(list[i], ctx))
                {
                    return i;
                }
            }
            return -1;
        }

        public static bool RemoveFirstOf<T, C>(this SimpleCollection<T> list, C ctx, Func<T, C, bool> match)
        {
            var index = list.FindIndex(ctx, match);
            if (index!= -1)
            {
                list.RemoveAt(index);
                return true;
            }
            return false;
        }

        public static bool RemoveLastOf<T, C>(this SimpleCollection<T> list, C ctx, Func<T, C, bool> match)
        {
            var index = list.FindLastIndex(ctx, match);
            if (index!= -1)
            {
                list.RemoveAt(index);
                return true;
            }
            return false;
        }

        public static int RemoveAllNull<T>(this SimpleCollection<T> list) where T : class
        {
            var count = list.Count;
            var removeCount = 0;
            for (var i = 0; i < count; ++i)
            {
                if (list[i] == null)
                {
                    var newCount = i++;
                    for (; i < count; ++i)
                    {
                        if (list[i]!= null)
                        {
                            list[newCount++] = list[i];
                        }
                    }
                    removeCount = count - newCount;
                    list.RemoveRange(newCount, removeCount);
                    break;
                }
            }
            return removeCount;
        }

        public static int RemoveAllNullUnordered<<fim_suffix> list.Count;
            var last = count - 1;
            var removeCount = 0;
            for (var i = 0; i <= last;)
            {
                if (list[i] == null)
                {
                    if (last!= i)
                    {
                        list[i] = list[last];
                    }
                    --last;
                    ++removeCount;
                }
                else
                {
                    ++i;
                }
            }
            if (removeCount > 0)
            {
                list.RemoveRange(count - removeCount, removeCount);
            }
            return removeCount;
        }

        public static int RemoveAll<T, C>(this SimpleCollection<T> list, C ctx, Func<T, C, bool> match)
        {
            var count = list.Count;
            var removeCount = 0;
            for (var i = 0; i < count; ++i)
            {
                if (match(list[i], ctx))
                {
                    var newCount = i++;
                    for (; i < count; ++i)
                    {
                        if (!match(list[i], ctx))
                        {
                            list[newCount++] = list[i];
                        }
                    }
                    removeCount = count - newCount;
                    list.RemoveRange(newCount, removeCount);
                    break;
                }
            }
            return removeCount;
        }

        public static int RemoveAllUnordered<T, C>(this SimpleCollection<T> list, C ctx, Func<T, C, bool> match)
        {
            var count = list.Count;
            var last = count - 1;
            var removeCount = 0;
            for (var i = 0; i <= last;)
            {
                if (match(list[i], ctx))
                {
                    if (last!= i)
                    {
                        list[i] = list[last];
                    }
                    --last;
                    ++removeCount;
                }
                else
                {
                    ++i;
                }
            }
            if (removeCount > 0)
            {
                list.RemoveRange(count - removeCount, removeCount);
            }
            return removeCount;
        }
    }
    
    public class SimpleCollection<T>: ObservableCollection<T>
    {
        public Type ValueType
        {
            get
            {
                return typeof(ICollection<T>);
            }
        }

        public void RemoveRange(int index, int count)
        {
            if (count > 0)
            {
                while (count > 0)
                {
                    count--;
                    if (this.Items.Count <= index)
                    {
                        break;
                    }
                    this.Items.RemoveAt(index);
                }
            }
        }


        public void ReplaceRange(IList<T> ts)
        {
            this.Clear();
            this.AddRange(ts);
        }


        public void AddEntriesFrom(CodedInputStream input, FieldCodec<T> codec)
        {
            uint tag = input.LastTag;
            var reader = codec.ValueReader;
            if (FieldCodec<T>.IsPackedRepeatedField(tag))
            {
                int length = input.ReadLength();
                if (length > 0)
                {
                    int oldLimit = input.PushLimit(length);
                    while (!input.ReachedLimit)
                    {
                        Add(reader(input));
                    }
                    input.PopLimit(oldLimit);
                }
            }
            else
            {
                do
                {
                    Add(reader(input));
                } while (input.MaybeConsumeTag(tag));
            }
        }

        public int CalculateSize(FieldCodec<T> codec)
        {
            if (Count == 0)
            {
                return 0;
            }
            uint tag = codec.Tag;
            if (codec.PackedRepeatedField)
            {
                int num = CalculatePackedDataSize(codec);
                return CodedOutputStream.ComputeRawVarint32Size(tag) + CodedOutputStream.ComputeLengthSize(num) + num;
            }
            Func<T, int> valueSizeCalculator = codec.ValueSizeCalculator;
            int num2 = Count * CodedOutputStream.ComputeRawVarint32Size(tag);
            for (int i = 0; i < Count; i++)
            {
                num2 += valueSizeCalculator(this[i]);
            }
            return num2;
        }

        private int CalculatePackedDataSize(FieldCodec<T> codec)
        {
            int fixedSize = codec.FixedSize;
            if (fixedSize == 0)
            {
                Func<T, int> valueSizeCalculator = codec.ValueSizeCalculator;
                int num = 0;
                for (int i = 0; i < Count; i++)
                {
                    num += valueSizeCalculator(this[i]);
                }
                return num;
            }
            return fixedSize * Count;
        }

        public void WriteTo(CodedOutputStream output, FieldCodec<T> codec)
        {
            if (Count == 0)
            {
                return;
            }
            Action<CodedOutputStream, T> valueWriter = codec.ValueWriter;
            uint tag = codec.Tag;
            if (codec.PackedRepeatedField)
            {
                uint value = (uint)CalculatePackedDataSize(codec);
                output.WriteTag(tag);
                output.WriteRawVarint32(value);
                for (int i = 0; i < Count; i++)
                {
                    valueWriter(output, this[i]);
                }
            }
            else
            {
                for (int j = 0; j < Count; j++)
                {
                    output.WriteTag(tag);
                    valueWriter(output, this[j]);
                }
            }
        }
    }
}<fim_middle>T>(this SimpleCollection<T> list) where T : class
        {
            var count =<|endoftext|>