import React, { useEffect } from "react";
import { createUseStyles } from "react-jss";
import NotFoundPage from "../../pages/404";
import AuthenticationStore from "../../stores/authentication";
import AdBanner from "../ad/adBanner";
import Avatar from "./components/avatar";
import Collections from "./components/collections";
import Creations from "./components/creations";
import Description from "./components/description";
import Friends from "./components/friends";
import Groups from "./components/groups";
import ProfileHeader from "./components/profileHeader";
import RobloxBadges from "./components/robloxBadges";
import Statistics from "./components/stats";
import UserProfileStore from "./stores/UserProfileStore";
import Favorites from "./components/favorites";
import HorizontalTabs from "../horizontalTabs";
import Badges from "./components/badges";

const useStyles = createUseStyles({
    profileContainer: {
        background: 'var(--background-color)',
        marginTop: '8px',
    },
    container: {
        maxWidth: '970px!important',
        '@media(max-width: 767px)': {
            padding: '0 5px',
        },
    },
    selectedElementClass: {
        marginTop: 0
    }
})

const UserProfile = props => {
    const s = useStyles();
    const store = UserProfileStore.useContainer();
    const auth = AuthenticationStore.useContainer();
    
    useEffect(() => {
        store.setUserId(props.userId);
    }, [props]);
    
    useEffect(() => {
        if (auth.isPending || !auth.userId || !store.userId) return;
        store.getFriendStatus(auth.userId);
    }, [store.userId, auth.userId, auth.isPending]);
    
    if (!store.userId || !store.userInfo || auth.isPending)
        return null;
    
    if (store.userInfo.isBanned)
        return <NotFoundPage/>;
    
    return <div className={`${s.container} container`}>
        <AdBanner/>
        <div className={s.profileContainer}>
            <ProfileHeader/>
            <HorizontalTabs options={[
                {
                    name: 'About',
                    element: <>
                        <Description/>
                        <Avatar userId={store.userId}/>
                        <Friends/>
                        <Collections userId={store.userId}/>
                        <Groups/>
                        <Favorites userId={store.userId}/>
                        <RobloxBadges userId={store.userId}/>
                        <Badges userId={store.userId}/>
                        <Statistics/>
                    </>
                },
                {
                    name: 'Creations',
                    element: <>
                        <Creations/>
                    </>
                },
            ]} elementClass={s.selectedElementClass}/>
        </div>
    </div>
}

export default UserProfile;