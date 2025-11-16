import { useEffect, useState } from "react";
import { createUseStyles } from "react-jss";
import {getItemUrl} from "../../../services/catalog";
import useCardStyles from "../styles/card";
import Subtitle from "./subtitle";
import SmallTextLink from "./smallTextLink";
import useButtonWrapperStyle from '../styles/buttonWrapper'
import {getUserBadges, getUserBadgesBasic} from "../../../services/badges";
import ItemImage from "../../itemImage";
import Link from "../../link";
import {ModerationStatusStr} from "../../../models/enums";

const useBadgeStyles = createUseStyles({
  imageWrapper: {
    border: '1px solid var(--text-color-quinary)',
    //borderRadius: '4px',
    borderRadius: 0,
  },
  image: {
    width: '100%',
    margin: '0 auto',
    display: 'block',
    padding: 0
  },
  itemLabel: {
    fontWeight: 500,
    fontSize: '16px',
    textOverflow: 'ellipsis',
    overflow: 'hidden',
    whiteSpace: 'nowrap',
  },
  labelWrapper: {
    width: '100%',
    marginTop: '-27px',
    overflow: 'hidden',
  },
  overlayLimited: {
    height: '28px',
    marginLeft: '-14px',
  },
  overlayLimitedUnique: {
    height: '28px',
    marginLeft: '-14px',
  },
});

const UserBadges = props => {
  const { userId } = props;
  const s = useBadgeStyles();
  const s2 =useButtonWrapperStyle();
  const cardStyles = useCardStyles();
  const [badges, setBadges] = useState(null);
  useEffect(() => {
    getUserBadges({ userId, limit: 6 }).then(d => setBadges(d.data.filter(d2 => d2.moderationStatus === ModerationStatusStr.ReviewApproved)));
  }, [userId]);

  if (!badges || setBadges.length === 0) {
    return null;
  }
  return <div className='flex'>
    <div className='col-10'>
      <Subtitle>Badges</Subtitle>
    </div>
    <div className='col-2'>
      <div className={`${s2.buttonWrapper}`}>
        <SmallTextLink href={`/users/${userId}/inventory`}>See All</SmallTextLink>
      </div>
    </div>
    <div className='col-12'>
      <div className={`marginStuff ${cardStyles.card}`}>
        <div className='flex ps-4 pe-4 pt-4 pb-4' style={{
          gap: 3,
          //justifyContent: 'center'
        }}>
          {
            badges.map((badgeInfo, i) => {
              /** @type BadgeEntry */
              const badge = badgeInfo;
              const assetId = badge.id;
              const url = getItemUrl({assetId: assetId, name: badge.name});

              return <div className='col-4 col-lg-2 ml-1 mr-1' style={{
                width: 'calc(16.66666667% - 3px)'
              }} key={i}>
                <Link href={url}>
                  <a href={url}>
                    <div className={s.imageWrapper}>
                      <ItemImage id={assetId} className={s.image}/>
                    </div>
                    <p className={`mb-0 link2019 ${s.itemLabel}`}>{badge.name}</p>
                  </a>
                </Link>
              </div>
            })
          }
        </div>
      </div>
    </div>
  </div>
}

export default UserBadges;