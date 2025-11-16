import CreateGame from "../../components/createGame";

const CreateGamePage = () => {
    return <CreateGame />
}

export const getStaticProps = () => {
    return {
        props: {
            title: 'Create Game - Pekora',
        },
    };
};

export default CreateGamePage;