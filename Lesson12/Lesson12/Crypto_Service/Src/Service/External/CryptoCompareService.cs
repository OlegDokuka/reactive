﻿using Lesson12.Crypto_Service.Src.Service.External.Utils;
using Lesson12.Lesson12.Crypto_Service_Idl.Src.Service;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Lesson12.Crypto_Service.Src.Service.External
{
    public class CryptoCompareService : ICryptoService
    {
        public static readonly int CACHE_SIZE = 3;

        private readonly IObservable<Dictionary<string, object>> _connectedClient;

        private readonly IObservable<Dictionary<string, object>> _reactiveCryptoListener;

        public CryptoCompareService(ILogger<CryptoCompareClient> logger, IEnumerable<IMessageUnpacker> messageUnpackers)
        {
            _connectedClient = ProvideCaching(new CryptoCompareClient(logger)
                    .Connect(
                        new List<string> { "5~CCCAGG~BTC~USD", "0~Coinbase~BTC~USD", "0~Cexio~BTC~USD" }.ToObservable(),
                        messageUnpackers.ToList()
                    )
                    // .Let(ProvideResilience)
                    .Do(m => Console.Out.WriteLine($"CryptoCompareService: Getting message: {m}")));
        }

        public IObservable<Dictionary<string, object>> EventsStream()
        {
            return _connectedClient;
        }

        // TODO: implement resilience such as retry with delay
        public static IObservable<T> ProvideResilience<T>(IObservable<T> input)
        {
            return input.RetryWhen(errors => errors.Delay(TimeSpan.FromSeconds(2)));
        }

        // TODO: implement caching of 3 last elements & multi subscribers support
        public static IObservable<T> ProvideCaching<T>(IObservable<T> input)
        {
            
            var connectableObservable = input.Multicast(new ReplaySubject<T>(3));
            connectableObservable.Connect();

            return connectableObservable;
        }
    }
}
