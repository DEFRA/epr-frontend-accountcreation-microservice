#!/usr/bin/env bash

dotnet test src/FrontendAccountCreation.Web.UnitTests/FrontendAccountCreation.Web.UnitTests.csproj --logger "trx;logfilename=testResults.trx"
dotnet test src/FrontendAccountCreation.Core.UnitTests/FrontendAccountCreation.Core.UnitTests.csproj --logger "trx;logfilename=testResults.trx"