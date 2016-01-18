﻿namespace Spreads.Tests.Series


open FsUnit
open NUnit.Framework

open System
open System.Linq
open System.Collections.Generic
open System.Diagnostics
open Spreads
open Spreads.Collections
open Spreads.Collections.Experimental
open System.Threading

type ReadmeTestsModule() =
    
  [<Test>]
  member this.CouldCalculateSimpleIndexInRealTime() =
    let numQuoteSources = 500

    let cts = new CancellationTokenSource()
    let ct = cts.Token
    let rng = System.Random()

    // monitoring variables
    let totalOutputs = ref 0
    let chunkOutputs = ref 0
    let totalInputs = ref 0
    let chunkInputs = ref 0

    let makeQuoteSources (n) : Series<DateTime, float>[] =
      let maps = Array.init n (fun _ -> SortedMap())
      let mutable value = 1.0
      let mutable ticks = 0L
      let task = async {
                while not ct.IsCancellationRequested do
                  let nextIdx = rng.Next(n)
                  let time = DateTime.UtcNow.AddTicks(Interlocked.Increment(&ticks))
                  Interlocked.Increment(totalInputs) |> ignore
                  Interlocked.Increment(chunkInputs) |> ignore
                  value <- value*(1.0 + rng.NextDouble()*0.002 - 0.001 + 0.001)
                  maps.[nextIdx].Add(time, value)
                  //do! Async.Sleep 1
            }
      Async.Start(task, ct)
      maps |> Array.map (fun x -> x.Repeat())


    let makeQuoteSource () : Series<DateTime, float> = // data is produced outside
        let mutable value = 1.0
        let sm = SortedMap()
        let now = DateTime.UtcNow
        let mutable trend = -1.0
        let mutable cnt = 0
        let gapMillisecs = int(rng.NextDouble() * 200.0 + 100.0)

        sm.Add(now, value)

        let task = async {
                        while not ct.IsCancellationRequested do
                            do! Async.Sleep gapMillisecs
                            let time = DateTime.UtcNow
                            Interlocked.Increment(totalInputs) |> ignore
                            Interlocked.Increment(chunkInputs) |> ignore
                            value <- value*(1.0 + rng.NextDouble()*0.002 - 0.001 + 0.001 * trend)
                            sm.Add(time, value)
                            cnt <- cnt + 1
                            if cnt % 40 = 0 then trend <- -trend
                    }
        Async.Start(task, ct)
        sm :> Series<DateTime, float>

    let quoteSources = makeQuoteSources(numQuoteSources)
//        Array.init numQuoteSources (fun i -> let qs = makeQuoteSource()
//                                             // make the 1st quotesource discrete, the rest continuous
//                                             //if i = 0 then qs else 
//                                             qs.Repeat()
//                                             )

    let index : Series<DateTime, float> = 
        quoteSources.Zip(fun k vArr -> vArr |> Array.average)

    // monitoring - 2s chunks, and total
    let chunkMillisecs = 2000
    let totalStopwatch = new Diagnostics.Stopwatch()
    let chunkStopwatch = new Diagnostics.Stopwatch()
    totalStopwatch.Start()
    chunkStopwatch.Start()

    let printRates (stopwatch : Diagnostics.Stopwatch) inputs outputs =
        let elapsedSeconds = (float)stopwatch.ElapsedMilliseconds / 1000.
        printfn "%A: %d inputs (%f/s) -> %d outputs (%f/s)" DateTime.UtcNow inputs (((float)inputs)/elapsedSeconds) outputs (((float)outputs)/elapsedSeconds) 
        printfn "Total memory: %d" (GC.GetTotalMemory(false))

    let monitor = async {
                        while not ct.IsCancellationRequested do
                            do! Async.Sleep chunkMillisecs
                            let inputs = Interlocked.Exchange(chunkInputs, 0)
                            let outputs = Interlocked.Exchange(chunkOutputs, 0)
                            printRates chunkStopwatch inputs outputs
                            chunkStopwatch.Restart()
                        printfn "Total:"
                        printRates totalStopwatch !totalInputs !totalOutputs
                        }
    Async.Start(monitor, ct)

    index.Do((fun k v -> 
                // just count the outputs
                Interlocked.Increment(chunkOutputs) |> ignore
                Interlocked.Increment(totalOutputs) |> ignore
                ), ct)

    Thread.Sleep(10000000)
    // Stop it gracefully
    //cts.Cancel()