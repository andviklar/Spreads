﻿// Learn more about F# at http://fsharp.net
// See the 'F# Tutorial' project for more help.
open System
open System.Collections.Generic
open System.Diagnostics
open Spreads
open Spreads.Collections
open Spreads.Collections.Experimental
open Spreads.Collections.Obsolete
  /// run f and measure ops per second
let perf (count:int64) (message:string) (f:unit -> unit) : unit = // int * int =
  GC.Collect(3, GCCollectionMode.Forced, true)
  let startMem = GC.GetTotalMemory(false)
  let sw = Stopwatch.StartNew()
  f()
  sw.Stop()
  let endtMem = GC.GetTotalMemory(true)
  let p = (1000L * count/sw.ElapsedMilliseconds)
  //int p, int((endtMem - startMem)/1024L)
  Console.WriteLine(message + ", #{0}, ops: {1}, mem/item: {2}", 
    count.ToString(), p.ToString(), ((endtMem - startMem)/count).ToString())
  //Console.WriteLine("Elapsed ms: " + sw.ElapsedMilliseconds.ToString())
let rng = Random()

[<CustomComparison;CustomEquality>]
type TestStruct =
  struct 
    val value : int64
    val n : int64
    internal new(value:int64, n:int64) = {value = value; n = n}
  end
  override x.Equals(yobj) =
    match yobj with
    | :? TestStruct as y -> (x.value = y.value && x.n = y.n)
    | _ -> false
  override x.GetHashCode() = x.value.GetHashCode()
  override x.ToString() = x.value.ToString()
  interface System.IComparable<TestStruct> with
    member x.CompareTo y = 
      let first = x.value.CompareTo(y.value)
      if  first = 0 then
        x.n.CompareTo(y.n)
      else first
  interface System.IComparable with
    member x.CompareTo other = 
      match other with
      | :? TestStruct as y -> x.value.CompareTo(y.value)
      | _ -> invalidArg "other" "Cannot compare values of different types"

type IInc =
  abstract Inc : unit -> int
  abstract Value: int with get

type MyInc =
  struct 
    val mutable value : int
    new(v:int) = {value = v}
  end
  member this.Inc() = this.value <- (this.value + 1);this.value
  member this.Value with get() = this.value
  interface IInc with
    member this.Inc() = this.value <- this.value + 1;this.value
    member this.Value with get() = this.value

[<StructAttribute>]
type MyInc2 =
  val mutable v : int
  new(v:int) = {v = v}

  member this.Inc() = this.v <- this.v + 1;this.v
  member this.Value with get() = this.v

  interface IInc with
    member this.Inc() = this.v <- this.v + 1;this.v
    member this.Value with get() = this.v


[<EntryPoint>]
let main argv = 
 
  let mutable mi = MyInc(0)
  mi.Inc() |> ignore
  Console.WriteLine(mi.Value)
//  let sd = new SortedDeque<int>()
//  sd.Add(1);
//  sd.Add(2);
//  sd.Add(3);
//  sd.Add(4);
//
//  for v in sd do
//    Console.WriteLine(v);


//  let count = 1000000L
//  let dc : IKeyComparer<int64> = SpreadsComparerInt64() :> IKeyComparer<int64> 
//
//  let smap = ref (Spreads.Collections.SortedMap(comparer = (dc :> IComparer<int64>)))
//  for i in 0L..count do
//    smap.Value.Add(i, i)
//
//  
//
//
//  for i in 0..99 do
//    let sdf = ref (Spreads.Collections.SortedMap(comparer = (dc :> IComparer<int64>)))
//    perf count "SortedMapRegular Add Double GC" (fun _ ->
//      for i in 0L..count do
//        sdf.Value.Add(i, double i)
//    )

//    perf count "SortedMapRegular Iterate" (fun _ ->
//        for i in smap.Value do
//          let res = i.Value
//          ()
//      )
//    perf count "SortedMapRegular Read" (fun _ ->
//      for i in 0L..count do
//        let res = smap.Value.Item(i)
//        if res <> i then failwith "SortedMap failed"
//        ()
//    )

//    perf count "SortedMapRegular Read as RO" (fun _ ->
//      let ro = smap.Value.ReadOnly() :> IReadOnlyOrderedMap<int64,int64>
//      for i in 0L..count do
//        let res = ro.Item(i)
//        if res <> i then failwith "SortedMap failed"
//        ()
//    )
////
//  OptimizationSettings.ArrayPool <- DoubleArrayPool()
//
//  for i in 0..99 do
//    let sdf = ref (Spreads.Collections.SortedMap(comparer = (dc :> IComparer<int64>)))
//    perf count "SortedMapRegular Add Double Pool" (fun _ ->
//      for i in 0L..count do
//        sdf.Value.Add(i, double i)
//    )

  //    let mutable res = 0L
//      perf count "SMR Iterate as RO+AddWithBind" (fun _ ->
//        let ro = smap.Value.ReadOnly().Add(123456L) :> IReadOnlyOrderedMap<int64,int64>
//        for i in ro do
//          res <- i.Value
//          ()
//      )
      //perf count "SMR Iterate with Zip" (fun _ ->
//      let ro = (((smap.Value.Zip(smap.Value, fun v v2 -> v + v2)))) :> IReadOnlyOrderedMap<int64,int64>
//      for i in ro do
//        let res = i.Value
//        ()
//      //)
//      Console.WriteLine(res)
//  for i in 0..4 do
//    perf 10000000L "Sequence expression" (fun _ ->
//      let newVector = [| 
//        for v in 1..10000000 ->
//          v |] 
//      ()
//    )
//  for i in 0..4 do
//    perf 10000000L "Preallocate" (fun _ ->
//      let newVector2 = 
//        let a = Array.zeroCreate 10000000
//        for v in 1..10000000 do
//          a.[v-1] <- v
//        a
//      ()
//    )
//
//  for i in 0..4 do
//    perf 10000000L "List" (fun _ ->
//      let newVector3 = 
//        let a = System.Collections.Generic.List() // do not set capacity
//        for v in 1..10000000 do
//          a.Add(v)
//        a.ToArray()
//      ()
//    )
  //Spreads.Tests.Collections.Benchmarks.CollectionsBenchmarks.SHM_regular_run() // .SortedMapRegularTest(10000000L)
//  Spreads.Tests.Experimental.Buckets
//    .``Could store N sorted maps with 1000 elements to MMDic``(10000L)
  //Console.ReadKey()
  //Spreads.Tests.Collections.Benchmarks.CollectionsBenchmarks.SortedDeque_run()
//  let count = 1000000L
//  let ss = SortedSet()
//  let heap = ref (Heap.empty<TestStruct>(false))
//  let sd = Extra.SortedDeque()
//  let sm = SortedMap()
//  let width = 10
//  for i in 0..(width-1) do
////    ss.Add(TestStruct(int64 (rng.Next(0, width-1)), int64 i)) |> ignore
////    sd.Add(TestStruct(int64 (rng.Next(0, width-1)), int64 i))
//    sm.Add(TestStruct(int64 (rng.Next(0, width-1)), int64 i), false)
////    heap := Heap.insert (TestStruct(int64 (rng.Next(0, width-1)), int64 i)) !heap
//  perf count "SortedSet rotation" (fun _ ->
//    for i in (int64 width)..(count+(int64 width)) do
////      let min = ss.Min
////      ss.Remove(min) |> ignore
////      ss.Add(TestStruct(int64 (rng.Next(0, width-1)), int64 i))|> ignore
//      let ok,min = sm.RemoveFirst()
//      sm.Add(TestStruct(int64 (rng.Next(0, width-1)), int64 i), false)
//      let min = sd.RemoveFirst()
//      sd.Add(TestStruct(int64 (rng.Next(0, width-1)), int64 i))
//      ()
//      let min, tail = Heap.uncons !heap
//      heap := Heap.insert (TestStruct(int64 (rng.Next(0, width-1)), int64 i)) tail
//      ()
    //Console.WriteLine(Heap.length !heap)
    //Console.WriteLine(ss.Count.ToString())
    //Console.WriteLine(sd.Count.ToString())
    //Console.WriteLine(sm.Count.ToString())
//  )
//  Console.ReadKey() |> ignore
//  GC.GetTotalMemory(false).ToString() |> Console.WriteLine
//  let wrArr = [| WeakReference(Array.init 10000000 id) |]
//  GC.GetTotalMemory(false).ToString() |> Console.WriteLine
//  GC.Collect(2, GCCollectionMode.Forced, true)
//  if wrArr.[0].IsAlive then Console.WriteLine "Alive" else Console.WriteLine "Dead" 
//
//  Console.ReadKey() |> ignore
//  if wrArr.[0].IsAlive then Console.WriteLine "Alive" else Console.WriteLine "Dead"
//  Console.ReadKey() |> ignore
//


//
//  let count = 1000000L
//  for n in 0..9 do
//
//    let shm = ref (SortedHashMap(Int64SortableHasher(1024)))
//    for i in 0L..count do
//      shm.Value.Add(i, i)
//
//    for i in 0L..count do
//      let res = shm.Value.Item(i)
//      if res <> i then failwith "SHM failed"
//      ()
//
//    for i in shm.Value do
//      let res = i.Value
//      if res <> i.Value then failwith "SHM failed"
//      ()

//
//    let shmt = ref (SortedHashMap(TimePeriodSortableHasher()))
//    let initDTO = DateTimeOffset(2014,11,23,0,0,0,0, TimeSpan.Zero)
//    for i in 0..(int count) do
//      shmt.Value.AddLast(TimePeriod(UnitPeriod.Second, 1, 
//                          initDTO.AddSeconds(float i)), i)
//    for i in 0..(int count) do
//      let res = shmt.Value.Item(TimePeriod(UnitPeriod.Second, 1, 
//                    initDTO.AddSeconds(float i)))
//      ()
//    Console.WriteLine(n.ToString())

//  let shmt = ref (SortedHashMap(TimePeriodSortableHasher()))
//  let initDTO = DateTimeOffset(2014,11,23,0,0,0,0, TimeSpan.Zero)
//  for i in 0..(int count) do
//    shmt.Value.Add(TimePeriod(UnitPeriod.Second, 1, 
//                      initDTO.AddSeconds(float i)), i)
//  for i in 0..9 do
//    for ii in shmt.Value do
//      let res = ii.Value
//      ()
//    Console.WriteLine(i.ToString())
  Console.ReadLine();
  printfn "%A" argv
  0 // return an integer exit code
