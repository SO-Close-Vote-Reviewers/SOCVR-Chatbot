FROM mono:latest

RUN apt-get update && apt-get install -y \
 python-software-properties \
 software-properties-common \
 nunit \
 git \
 nano \
 sudo \
 wget

# copy everything to the /srv folder
COPY ./ /srv/chatbot/

# compile it
RUN \
  wget https://dist.nuget.org/win-x86-commandline/latest/nuget.exe && \
  mono /nuget.exe restore /srv/chatbot/source/SOCVR.Chatbot.sln && \
  xbuild /p:Configuration=Release /srv/chatbot/SOCVR.Chatbot.sln

CMD ["mono", "/srv/chatbot/source/SOCVR.Chatbot/bin/Release/SOCVR.Chatbot.exe"]
