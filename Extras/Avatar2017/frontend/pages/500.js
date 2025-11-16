import ErrorPage from "../components/errorPage";

const errorCode = 500;
const errorTitle = "Internal Server Error";
const errorDesc = "An unexpected error occurred";

export default () => <ErrorPage title={errorTitle} desc={errorDesc} code={errorCode} />