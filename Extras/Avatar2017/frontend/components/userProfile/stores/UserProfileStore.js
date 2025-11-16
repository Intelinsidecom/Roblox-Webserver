import { useEffect, useState } from "react";
import { createContainer } from "unstated-next";
import getFlag from "../../../lib/getFlag";
import {
    getFollowersCount,
    getFollowingsCount,
    getFriends,
    getFriendStatus,
    isAuthenticatedUserFollowingUserId
} from "../../../services/friends";
import { getUserGames, multiGetPlaceDetails } from "../../../services/games";
import { getUserGroups } from "../../../services/groups";
import { getPreviousUsernames, getUserInfo, getUserStatus } from "../../../services/users";
import { multiGetUserThumbnails3D } from "../../../services/thumbnails";
import { Stopwatch, wait } from "../../../lib/utils";
import request from "../../../lib/request";
import { FeedbackType } from "../../../models/feedback";
import FeedbackStore from "../../../stores/feedback";
import { getUserConnections } from "../../../services/accountInformation";

const UserProfileStore = createContainer(() => {
    const [userId, setUserId] = useState(null);
    const [username, setUsername] = useState(null);
    const [lastError, setLastError] = useState(null);
    const [userInfo, setUserInfo] = useState(null);
    const [userConns, setUserConns] = useState({});
    const [userAv3D, setUserAv3D] = useState(null);
    const [status, setStatus] = useState(null);
    const [previousNames, setPreviousNames] = useState(null);
    const [friends, setFriends] = useState(null);
    const [followersCount, setFollowersCount] = useState(null);
    const [followingsCount, setFollowingsCount] = useState(null);
    const [friendStatus, setFriendStatus] = useState(null);
    const [groups, setGroups] = useState(null);
    const [createdGames, setCreatedGames] = useState(null);
    const [tab, setTab] = useState('About');
    const [isFollowing, setIsFollowing] = useState(null);
    const [RAP, setRAP] = useState(null);
    const [verified, setVerified] = useState(false);
    
    const feedback = FeedbackStore.useContainer();
    
    async function GetUserThumb3D(userId) {
        await setUserAv3D(null);
        let attempts = 0;
        let stopwatch = new Stopwatch();
        stopwatch.Start();
        while (attempts <= 10 && userAv3D === null) {
            let thumbnail = await multiGetUserThumbnails3D({userIds: [userId]})
                .then(result => result[0]);
            if (thumbnail.state === "Completed" && typeof thumbnail.imageUrl === "string") {
                /** @type Thumbnail3D */
                let thumb = (await request("GET", thumbnail.imageUrl)).data;
                if (thumb?.textures?.length && thumb.textures.length > 0) {
                    setUserAv3D(thumb);
                    break;
                }
            } else {
                console.warn("User thumbnail 3D has not completed rendering yet.");
            }
            attempts++;
            await wait(1);
        }
        stopwatch.Stop();
        if (attempts > 10 && userAv3D == null) {
            feedback.addFeedback("Could not get this user's 3D avatar render. Please try again later.", FeedbackType.ERROR);
        } else {
            console.log(`Got 3D avatar render in ${stopwatch.ElapsedMilliseconds()}ms, in ${attempts} attempts.`);
        }
    }
    
    useEffect(() => {
        if (!userId) return;
        getUserInfo({ userId }).then(result => {
            setUserInfo(result);
            setUsername(result.name);
        }).catch(e => {
            setLastError('InvalidUserId');
        });
        getPreviousUsernames({ userId: userId }).then(setPreviousNames);
        if (getFlag('userProfileUserStatusEnabled', false))
            getUserStatus({ userId }).then(setStatus);
        getFollowersCount({ userId }).then(setFollowersCount);
        getFollowingsCount({ userId }).then(setFollowingsCount);
        getFriends({ userId }).then(setFriends);
        getUserGroups({ userId }).then(setGroups);
        getUserGames({ userId, cursor: '' }).then(d => {
            setCreatedGames(d.data);
        });
        isAuthenticatedUserFollowingUserId({
            userId,
        }).then(setIsFollowing);
        getUserConnections({ userId, returnUrls: true }).then(setUserConns);
        GetUserThumb3D(userId);
    }, [userId]);
    
    useEffect(() => {
        if (userAv3D === null) {
            GetUserThumb3D(userId);
        }
    }, [userAv3D]);
    
    useEffect(() => {
        return () => {
            setUserAv3D({});
        };
    }, []);
    
    return {
        userId,
        setUserId,
        
        lastError,
        setLastError,
        
        username,
        RAP,
        userInfo,
        
        status,
        setStatus,
        
        previousNames,
        setPreviousNames,
        
        followersCount,
        setFollowersCount,
        followingsCount,
        setFollowingsCount,
        
        friends,
        setFriends,
        friendStatus,
        setFriendStatus,
        
        groups,
        setGroups,
        
        createdGames,
        setCreatedGames,
        
        tab,
        setTab,
        
        isFollowing,
        setIsFollowing,
        
        /** @type UserConnection */
        userConns,
        /** @type Thumbnail3D */
        userAv3D,
        
        getFriendStatus: (authenticatedUserId) => {
            getFriendStatus({ authenticatedUserId, userId }).then(setFriendStatus);
        },
    }
});

export default UserProfileStore;