using System.Windows.Forms;

public class BaseForm : Form
{
    public BaseForm()
    {
        SETUNA.Main.FormManager.RegisterForm(this);
    }

    ~BaseForm()
    {
        SETUNA.Main.FormManager.DeregisterForm(this);
    }
}
