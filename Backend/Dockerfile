FROM python:3.9.13-slim
RUN set -ex \
&& sed -i 's/deb.debian.org/mirrors.ustc.edu.cn/g' /etc/apt/sources.list \
&& sed -i 's|security.debian.org/debian-security|mirrors.ustc.edu.cn/debian-security|g' /etc/apt/sources.list \
&& apt-get update \
&& apt-get install ffmpeg -y \
&& apt-get install redis -y \
&& apt-get install vim -y
#RUN chmod -R +777 /etc/redis
#RUN sed -i 's/protected-mode yes/protected-mode no/g' /etc/redis/redis.conf
#RUN sed -i 's/# requirepass foobared/requirepass "passwd"/g' /etc/redis/redis.conf
#RUN sed -i 's/port 6379/port 6380/g' /etc/redis/redis.conf
#RUN service redis-server stop
#RUN redis-server /etc/redis/redis.conf --daemonize yes
COPY requirements.txt /tmp/
RUN set -ex \
&& pip config set global.index-url https://mirrors.cloud.tencent.com/pypi/simple \
&& pip install -r /tmp/requirements.txt
RUN adduser --disabled-password --gecos '' deepsoft \
    && usermod -u 1000 deepsoft && usermod -G 1000 deepsoft
WORKDIR /home/deepsoft/code
USER deepsoft
