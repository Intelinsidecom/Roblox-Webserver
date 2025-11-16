import React from "react";
import { createUseStyles } from "react-jss";
import LoginModalStore from "../../../stores/loginModal";
import LoginModal from "../../loginModal";
import getFlag from "../../../lib/getFlag";
import { useRouter } from "next/dist/client/router";

const useLoginAreaStyles = createUseStyles({
  text: {
    color: 'white',
    fontWeight: 400,
    fontSize: '16px',
    borderBottom: 0,
    margin: 0,
    textAlign: 'right',
    whiteSpace: 'nowrap',
  },
  link: {
    color: 'white',
    textDecoration: 'none',
    padding: '6px 9px',
    userSelect: 'none',
    '&:hover': {
      color: 'white',
      background: 'rgba(25,25,25,0.1)',
      cursor: 'pointer',
      borderRadius: '4px',
    },
  },
  signupLink: {
    backgroundColor: '#02b757!important',
    borderRadius: '3px!important',
    '&:hover': {
      backgroundColor: '#3fc679!important',
      borderRadius: '3px!important',
    }
  },

  loginContainer: {
    maxWidth: '25%',
    float: 'right',
    marginLeft: 'auto',
    marginRight: '12px',
  },
  buttonContainer: {
    justifyContent: 'center',
    alignItems: 'center',
    height: '100%',
  },
});

const LoginArea = props => {
  const Router = useRouter();
  const s = useLoginAreaStyles();
  const loginModalStore = LoginModalStore.useContainer();

  return <div className={`row ${s.loginContainer}`}>
    <div className='col-6 offset-6'>
      <div className={`row ${s.buttonContainer}`}>
        <div className='col-6'>
          <p className={s.text}>
            <a className={s.link} onClick={(e) => {
              e.preventDefault();
              if (getFlag('requireLoginThroughCookie', true)) {
                if (getFlag('clientSideRenderingEnabled', false)) {
                  Router.push('/login');
                } else {
                  window.location.href = '/login';
                }
                return;
              }
              loginModalStore.setOpen(!loginModalStore.open);
            }}>
              Login
            </a>
          </p>
          {loginModalStore.open && <LoginModal></LoginModal>}
        </div>
        <div className='col-6'>
          <p className={s.text}>
            <a className={`${s.link} ${s.signupLink}`} href='/auth/application'>
              Sign Up
            </a>
          </p>
        </div>
      </div>
    </div>
  </div>
}

export default LoginArea;