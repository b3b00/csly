FROM brendanburns/wasm-dev-base:0.0.6

# Dotnet
RUN apt install lld-14 -y -qq
RUN curl https://packages.microsoft.com/config/ubuntu/22.10/packages-microsoft-prod.deb -L --output packages-microsoft-prod.deb && \
    dpkg -i packages-microsoft-prod.deb && \
    rm packages-microsoft-prod.deb && \
    apt-get update && apt-get install -y dotnet-sdk-7.0

