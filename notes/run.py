import os
from IPython.display import HTML, display


def progress(value, max=100):
    return HTML("""
        <progress
            value='{value}'
            max='{max}',
            style='width: 100%'
        >
            {value}
        </progress>
    """.format(value=value, max=max))


pro_bar = display(progress(0, 100), display_id=True)

try:
    import google.colab
    IN_COLAB = True
except ImportError:
    IN_COLAB = False

if IN_COLAB:
    with open('frame-buffer', 'w') as writefile:
        writefile.write("""#taken from https://gist.github.com/jterrace/2911875
XVFB=/usr/bin/Xvfb
XVFBARGS=":1 -screen 0 1024x768x24 -ac +extension GLX +render -noreset"
PIDFILE=./frame-buffer.pid
case "$1" in
  start)
    echo -n "Starting virtual X frame buffer: Xvfb"
    /sbin/start-stop-daemon --start --quiet --pidfile $PIDFILE --make-pidfile --background --exec $XVFB -- $XVFBARGS
    echo "."
    ;;
  stop)
    echo -n "Stopping virtual X frame buffer: Xvfb"
    /sbin/start-stop-daemon --stop --quiet --pidfile $PIDFILE
    rm $PIDFILE
    echo "."
    ;;
  restart)
    $0 stop
    $0 start
    ;;
  *)
        echo "Usage: /etc/init.d/xvfb {start|stop|restart}"
        exit 1
esac
exit 0
    """)
    pro_bar.update(progress(5, 100))
    os.system('apt-get install daemon >/dev/null 2>&1')
    pro_bar.update(progress(10, 100))
    os.system('apt-get install wget >/dev/null 2>&1')
    pro_bar.update(progress(20, 100))
    os.system('wget http://security.ubuntu.com/ubuntu/pool/main/libx/libxfont/libxfont1_1.5.1-1ubuntu0.16.04.4_amd64.deb >/dev/null 2>&1')
    pro_bar.update(progress(30, 100))
    os.system('wget --output-document xvfb.deb http://security.ubuntu.com/ubuntu/pool/universe/x/xorg-server/xvfb_1.18.4-0ubuntu0.12_amd64.deb >/dev/null 2>&1')
    pro_bar.update(progress(40, 100))
    os.system('dpkg -i libxfont1_1.5.1-1ubuntu0.16.04.4_amd64.deb >/dev/null 2>&1')
    pro_bar.update(progress(50, 100))
    os.system('dpkg -i xvfb.deb >/dev/null 2>&1')
    pro_bar.update(progress(70, 100))
    os.system('rm libxfont1_1.5.1-1ubuntu0.16.04.4_amd64.deb')
    pro_bar.update(progress(80, 100))
    os.system('rm xvfb.deb')
    pro_bar.update(progress(90, 100))
    os.system('bash frame-buffer start')
    os.environ["DISPLAY"] = ":1"
pro_bar.update(progress(100, 100))
