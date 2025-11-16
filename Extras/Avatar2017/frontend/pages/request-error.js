import {useRouter} from "next/router";
import ErrorJS from './_error'

export default function RequestError() {
    const router = useRouter();
    return <ErrorJS statusCode={router?.query?.code} />
}