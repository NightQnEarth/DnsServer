using System;
using CommandLine;

namespace DnsServer
{
    public static class UIDataParser
    {
        // ReSharper disable once ParameterTypeCanBeEnumerable.Global
        public static Options GetInputData(string[] args)
        {
            var options = new Options();

            Parser.Default.ParseArguments<Options>(args)
                  .WithParsed(inputOptions => options.OriginDnsServerName = inputOptions.OriginDnsServerName)
                  .WithNotParsed(errors => Environment.Exit(0));

            return options;
        }
    }
}