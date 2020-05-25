using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.Logging;
using Logger = Sora.Utilities.Logger;

namespace Sora.Services
{
    internal class PluginAssemblyLoadContext : AssemblyLoadContext
    {
        protected override Assembly Load(AssemblyName assemblyName) => null;
    }

    public class PluginService
    {
        private readonly PluginAssemblyLoadContext _context
            = new PluginAssemblyLoadContext();

        private readonly Dictionary<Assembly, IPlugin> _entryPoints
            = new Dictionary<Assembly, IPlugin>();

        private readonly EventManager _ev;
        private readonly ApplicationPartManager _appPartManager;
        private readonly ILogger<PluginService> _logger;

        private readonly Dictionary<string, Assembly> _loadedPlugins
            = new Dictionary<string, Assembly>();

        public PluginService(EventManager ev, ApplicationPartManager appPartManager,
            ILogger<PluginService> logger)
        {
            _ev = ev;
            _appPartManager = appPartManager;
            _logger = logger;
        }

        public bool LoadPlugin(IApplicationBuilder app, string filename, bool isDep = false)
        {
            try
            {
                using var strm = File.OpenRead(filename); // Load from stream instead, allow us to live reload.
                var asm = _context.LoadFromStream(strm);
                if (asm == null) return false;

                _loadedPlugins.Add(filename, asm);
                _ev.LoadAssembly(asm);
                _logger.LogInformation("Loaded new Assembly {asm}", asm);

                // Register ASP.NET Core Controllers
                _appPartManager.ApplicationParts.Add(new AssemblyPart(asm));

                if (!isDep)
                    GetEntryPoint(asm)?.OnEnable(app);

                return true;
            }
            catch (Exception ex)
            {
                Logger.Err(ex);
                return false;
            }
        }

        public IPlugin GetEntryPoint(Assembly asm)
        {
            if (_entryPoints.ContainsKey(asm))
                return _entryPoints[asm];

            var iplug = asm.GetTypes()
                .FirstOrDefault(
                    x => x.IsClass &&
                         (x.BaseType == typeof(IPlugin) ||
                          x.BaseType == typeof(Plugin))
                );

            if (iplug == null)
            {
                Logger.Warn("No entry point found for plugin", asm);
                _entryPoints.Add(asm, null);
                return null;
            }

            var tArgs = (from cInfo in iplug.GetConstructors()
                from pInfo in cInfo.GetParameters()
                select _ev.Provider.GetService(pInfo.ParameterType)).ToArray();

            if (tArgs.Any(x => x == null))
                throw new Exception("Could not find Dependency, are you sure you registered the Dependency?");

            var retVal = Activator.CreateInstance(iplug, tArgs) as IPlugin;

            _entryPoints.Add(asm, retVal);

            return retVal;
        }

        public bool UnloadPlugins()
        {
            try
            {
                _context.Unload();
                return true;
            }
            catch (Exception ex)
            {
                Logger.Err(ex);
                return false;
            }
        }
    }
}