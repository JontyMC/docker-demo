FROM jontymc/hello

ENV DNX_VERSION=1.0.0-rc1-final DNX_USER_HOME=/opt/dnx
RUN dnu commands install Microsoft.Dnx.Watcher $DNX_VERSION --packages $DNX_USER_HOME/bin

ENTRYPOINT /opt/dnx/bin/dnx-watch web