version: 1.0
builds:
  Frontline Rivals Server: # replace with the name for your build
    executableName: FrontlineRivalsServer.x86_64 # the name of your build executable
    buildPath: C:\Users\andre\Documents\GameDesign\Progetti\Frontline Rivals\Frontline Rivals Server # the location of the build files
buildConfigurations:
  Frontline Rivals Server Configuration: # replace with the name for your build configuration
    build: Frontline Rivals Server # replace with the name for your build
    queryType: sqp # sqp or a2s, delete if you do not have logs to query
    binaryPath: FrontlineRivalsServer.x86_64 # the name of your build executable
    commandLine: -port $$port$$ -queryport $$query_port$$ -log $$log_dir$$/Engine.log # launch parameters for your server
    variables: {}
    cores: 1 # number of cores per server
    speedMhz: 180 # launch parameters for your server
    memoryMiB: 500 # launch parameters for your server
fleets:
  Card Battle Fleet:
    buildConfigurations:
      - Frontline Rivals Server Configuration
    regions:
      Europe:
        minAvailable: 1 # minimum number of servers running in the region
        maxServers: 1 # maximum number of servers running in the region
