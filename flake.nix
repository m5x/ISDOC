{
  description = "Nix flake for IsdocPdf development";

  inputs = {
    nixpkgs.url = "github:nixos/nixpkgs?ref=nixos-unstable";
  };

  outputs = { nixpkgs, ... } @ inputs:
  let
    system = "x86_64-linux";
    pkgs = import nixpkgs { inherit system; config.allowUnfree = true; };

    # .NET SDK
    dotnet_sdk = pkgs.dotnetCorePackages.sdk_9_0_1xx;
  in
  {
    devShells.${system}.default = (pkgs.buildFHSEnv {
          name = "devShell-Jean";

          targetPkgs = pkgs: with pkgs; [
            # basic dev env packages
            bash
            mprocs
            nushell

            # .NET-specific packages
            dotnet_sdk
            #icu icu.dev
            jetbrains.rider
            sourcegit
          ];

          profile = ''
            export DOTNET_ROOT="${dotnet_sdk}"
            export DOTNET_CLI_TELEMETRY_OPTOUT=1
            
            # .NET does not seem to parse TERMINFO_DIRS so TERMINFO must be properly set
            export TERMINFO="/run/current-system/sw/share/terminfo"

            # Make sure to always use the same locale during development.
            # C is used because it's English with sane defaults independent of politics.
            export LANG="C.UTF-8"

            # remove duplicate commands from Bash shell command history
            #export HISTCONTROL=ignoreboth:erasedups

            # do not pollute $HOME with config files (.build-tools directory is ignored in .gitignore)
            #export DOTNET_CLI_HOME=".build-tools/net_cli_home";
            #export NUGET_PACKAGES=".build-tools/nuget_packages";

            # Put dotnet tools in $PATH to be able to use them
            #export PATH="$DOTNET_CLI_HOME/.dotnet/tools:$PATH"

            # Without this, there are warnings about LANG, LC_ALL and locales.
            # Many tests fail due those warnings showing up in test outputs too...
            # This solution is from: https://gist.github.com/aabs/fba5cd1a8038fb84a46909250d34a5c1
            #export LOCALE_ARCHIVE="${pkgs.glibcLocales}/lib/locale/locale-archive"
          '';

          runScript = "nu";
        }).env;
  };
}
