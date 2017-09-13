﻿using Bara.Abstract.Cache;
using System;
using System.Collections.Generic;
using System.Text;
using Bara.Abstract.Core;
using Bara.Core.Context;
using Microsoft.Extensions.Logging;
using Bara.Model;

namespace Bara.Core.Cache
{
    public class CacheManager : ICacheManager
    {
        private readonly ILogger _logger;
        private static readonly object syncobj = new object();
        public CacheManager(ILoggerFactory loggerFactory, IBaraMapper baraMapper)
        {
            this._logger = loggerFactory.CreateLogger<CacheManager>();
            this.BaraMapper = baraMapper;
            RequestQueue = new Queue<RequestContext>();
        }

        private IDictionary<String, IList<Statement>> _mappedTriggerFlushs;
        public IDictionary<String, IList<Statement>> MappedTriggerFlushs
        {
            get
            {
                if (_mappedTriggerFlushs == null)
                {
                    lock (syncobj)
                    {
                        if (_mappedTriggerFlushs == null)
                        {
                            _logger.LogDebug("CacheManger Start Load MappedTriggerFlushs.");
                            _mappedTriggerFlushs = new Dictionary<String, IList<Statement>>();
                            foreach (var baraMap in BaraMapper.BaraMapConfig.BaraMaps)
                            {
                                foreach (var statement in baraMap.Statements)
                                {
                                    if (statement.Cache == null
                                        || statement.Cache.FlushOnExecutes == null)
                                    {
                                        continue;
                                    }
                                    foreach (var triggerStatement in statement.Cache.FlushOnExecutes)
                                    {
                                        IList<Statement> triggerStatements = null;
                                        if (_mappedTriggerFlushs.ContainsKey(triggerStatement.Statement))
                                        {
                                            triggerStatements = _mappedTriggerFlushs[triggerStatement.Statement];
                                        }
                                        else
                                        {
                                            triggerStatements = new List<Statement>();
                                            _mappedTriggerFlushs[triggerStatement.Statement] = triggerStatements;
                                        }
                                        triggerStatements.Add(statement);
                                    }
                                }
                            }
                            _logger.LogDebug("CacheManger End Load MappedTriggerFlushs.");
                        }
                    }
                }

                return _mappedTriggerFlushs;
            }
        }

        public object this[RequestContext context, Type type] { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public IBaraMapper BaraMapper { get; set; }
        public Queue<RequestContext> RequestQueue { get; set; }

        public void Enqueue(RequestContext context)
        {
            RequestQueue.Enqueue(context);
        }

        public void ClearQueue()
        {
            throw new NotImplementedException();
        }

        public void FlushQueue()
        {
            while (RequestQueue.Count > 0)
            {
                Flush(RequestQueue.Dequeue());
            }
        }

        private void Flush(RequestContext context)
        {
            String FullSqlId = context.FullSqlId;

        }

        public void ResetMappedCaches()
        {
            throw new NotImplementedException();
        }

        public void TriggerFlush(RequestContext context)
        {
            throw new NotImplementedException();
        }
    }
}
