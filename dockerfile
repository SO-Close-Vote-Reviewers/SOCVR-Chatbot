FROM mono:latest

RUN apt-get update && apt-get install -y \
 git \
 wget

# copy everything to the /srv folder
COPY ./ /srv/chatbot/

RUN wget https://dist.nuget.org/win-x86-commandline/latest/nuget.exe

# compile it
RUN \
  sed -i 's:\.designer\.cs:\.Designer\.cs:g' /srv/chatbot/source/SOCVR.Chatbot/SOCVR.Chatbot.csproj && \
  mono /nuget.exe restore /srv/chatbot/source/SOCVR.Chatbot.sln && \
  xbuild /p:Configuration=Release /srv/chatbot/source/SOCVR.Chatbot.sln

WORKDIR /srv/chatbot

CMD ["mono", "/srv/chatbot/source/SOCVR.Chatbot/bin/Release/SOCVR.Chatbot.exe"]
