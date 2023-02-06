using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace Order.Domain.Interfaces.Repositories.DataConnector
{
    internal interface IConfiguring : IConfiguration 
    {
        IConfiguration configuration { get; }
    }
}
