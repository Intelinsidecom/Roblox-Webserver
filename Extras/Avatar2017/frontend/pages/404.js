import ErrorPage from "../components/errorPage";

const errorCode = 404;
const errorTitle = "Page cannot be found or no longer exists";
const errorDesc = "Page Not found";

export default () => <ErrorPage title={errorTitle} desc={errorDesc} code={errorCode} />
