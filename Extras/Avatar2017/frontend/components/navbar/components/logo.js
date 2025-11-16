import { createUseStyles } from "react-jss";
import NavigationStore from "../../../stores/navigation";

const useLogoStyles = createUseStyles({
  imgDesktop: {
    width: '122px',
    minWidth: '122px',
    maxWidth: '122px',
    height: '40px',
    // backgroundImage: `url(/img/roblox_logo.svg)`,
    //backgroundImage: 'url(/img/holiday/projex_logo_studio.png)',
    backgroundImage: `url(/img/logo.png)`,
    // backgroundSize: '122px 30px',
    backgroundSize: "100% auto",
    display: 'none',
    '@media(min-width: 1325px)': {
      display: 'block',
    },
    backgroundRepeat: 'no-repeat',
    backgroundPosition: 'center'
  },
  imgMobile: {
    //backgroundImage: `url(/img/logo_R.svg)`,
    backgroundImage: 'url(/img/favicon.png)',
    width: '30px',
    height: '30px',
    display: 'block',
    backgroundSize: '30px',
    backgroundRepeat: 'no-repeat',
    backgroundPosition: 'center',
    marginLeft: '6px',
    '@media(min-width: 1325px)': {
      display: 'none',
    },
  },
  imgMobileWrapper: {
    marginLeft: '12px',
  },
  col: {
    maxWidth: '118px',
    padding: '0',
    margin: '0 12px',
    display: 'flex',
    justifyContent: 'start',
    alignItems: 'center',
    '@media(max-width: 1324px)': {
      margin: '0 6px',
      width: 'auto',
    },
    '@media(max-width: 992px)': {
      width: '20%',
      margin: 0,
      marginBottom: '6px',
    },
  },
  openSideNavMobile: {
    display: 'none',
    '@media(max-width: 1324px)': {
      display: 'block',
      float: 'left',
      height: '30px',
      width: '30px',
      cursor: 'pointer',
    },
  },
});
const Logo = () => {
  const s = useLogoStyles();
  const navStore = NavigationStore.useContainer();

  return <div className={`${s.col} col-3 col-lg-3`}>
    <div className={s.openSideNavMobile + ' icon-menu'} onClick={() => {
      navStore.setIsSidebarOpen(!navStore.isSidebarOpen);
    }}></div>
    <a className={s.imgDesktop} href='/home'></a>
    <a className={s.imgMobile} href='/home'></a>
    {/*<div className={s.imgMobileWrapper}>
    
    </div>*/}
  </div>
}

export default Logo;